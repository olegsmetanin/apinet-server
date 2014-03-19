using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AGO.Core;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Controllers.Security;
using AGO.Core.DataAccess;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Core.Modules.Attributes;
using AGO.Core.Security;
using AGO.Tasks.Controllers.DTO;
using AGO.Tasks.Model.Task;
using NHibernate.Criterion;

namespace AGO.Tasks.Controllers
{
	public class ProjectController: AbstractTasksController
	{
		public ProjectController(
			IJsonService jsonService, 
			IFilteringService filteringService, 
			ICrudDao crudDao, 
			IFilteringDao filteringDao, 
			ISessionProvider sessionProvider, 
			ILocalizationService localizationService, 
			IModelProcessingService modelProcessingService, 
			AuthController authController,
			ISecurityService securityService,
			ISessionProviderRegistry registry,
			DaoFactory factory)
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, localizationService, modelProcessingService, authController, securityService, registry, factory)
		{
		}

		[JsonEndpoint, RequireAuthorization]
		public ProjectDTO GetProject([NotEmpty] string project)
		{
			var fb = _FilteringService.Filter<ProjectModel>();
			var codePredicate = ApplyReadConstraint<ProjectModel>(project, fb.Where(m => m.ProjectCode == project));
			var p = DaoFactory.CreateMainFilteringDao().Find<ProjectModel>(codePredicate);

			if (p == null)
				throw new NoSuchProjectException();

			var adapter = new ProjectAdapter(_LocalizationService, CurrentUser);
			return adapter.Fill(p);
		}

		[JsonEndpoint, RequireAuthorization]
		public UpdateResult<ProjectDTO> UpdateProject([NotEmpty] string project, [NotNull] PropChangeDTO data)
		{
			return Edit<ProjectModel, ProjectDTO>(data.Id, project, (p, vr) =>
			{
				if (data.Prop.IsNullOrWhiteSpace())
				{
					throw new RequiredValueException();
				}

				try
				{
					switch (data.Prop)
					{
						case "Name":
							p.Name = data.Value.ConvertSafe<string>().TrimSafe();
							break;
						case "Description":
							p.Description = data.Value.ConvertSafe<string>().TrimSafe();
							break;
						case "VisibleForAll":
							p.VisibleForAll = data.Value.ConvertSafe<bool>();
							break;
						case "Status":
							//TODO business logic, security and other checks
							var newStatus = data.Value.ConvertSafe<ProjectStatus>();
							p.ChangeStatus(newStatus, CurrentUser);//create entity saved via cascade
							break;
						default:
							throw new UnsupportedPropertyForUpdateException(data.Prop);
					}
				}
				catch (InvalidCastException cex)
				{
					vr.AddFieldErrors(data.Prop, cex.GetBaseException().Message);
				}
				catch (OverflowException oex)
				{
					vr.AddFieldErrors(data.Prop, oex.GetBaseException().Message);
				}
				catch (TasksException ex)
				{
					vr.AddFieldErrors(data.Prop, ex.GetBaseException().Message);
				}
			},
			p => new ProjectAdapter(_LocalizationService, CurrentUser).Fill(p),
			() => { throw new ProjectCreationNotSupportedException(); });
		}

		[JsonEndpoint, RequireAuthorization]
		public bool TagProject(
			[NotEmpty] Guid modelId,
			[NotEmpty] Guid tagId)
		{
			var dao = DaoFactory.CreateMainCrudDao();
			var project = dao.Get<ProjectModel>(modelId, true);
			SecurityService.DemandUpdate(project, project.ProjectCode, CurrentUser.Id, MainSession);

			var link = project.Tags.FirstOrDefault(l => l.Tag.Id == tagId);

			if (link != null)
				return false;

			var tag = dao.Get<ProjectTagModel>(tagId, true);
			link = new ProjectToTagModel
			{
				Project = project,
				Tag = tag
			};
			SecurityService.DemandUpdate(link, project.ProjectCode, CurrentUser.Id, MainSession);
			project.Tags.Add(link);
			dao.Store(link);

			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public bool DetagProject(
			[NotEmpty] Guid modelId,
			[NotEmpty] Guid tagId)
		{
			var dao = DaoFactory.CreateMainCrudDao();
			var project = dao.Get<ProjectModel>(modelId, true);
			SecurityService.DemandUpdate(project, project.ProjectCode, CurrentUser.Id, MainSession);

			var link = project.Tags.FirstOrDefault(l => l.Tag.Id == tagId);
			if (link == null)
				return false;

			SecurityService.DemandDelete(link, project.ProjectCode, CurrentUser.Id, MainSession);
			project.Tags.Remove(link);
			dao.Delete(link);

			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<ProjectMemberDTO> GetMembers([NotEmpty] string project, string term, [InRange(0, null)] int page)
		{
			var criteria = PrepareLookup<ProjectMemberModel>(project, term, page, m => m.FullName);
			var adapter = new ProjectMemberAdapter(_LocalizationService);
			return criteria.List<ProjectMemberModel>().Select(adapter.Fill).ToList();
		}

		[JsonEndpoint, RequireAuthorization]
		public int GetMembersCount([NotEmpty] string project, string term)
		{
			var criteria = PrepareLookup<ProjectMemberModel>(project, term, 0, m => m.FullName);
			criteria.ClearOrders();
			return criteria.SetProjection(Projections.RowCount()).UniqueResult<int>();
		}

		[JsonEndpoint, RequireAuthorization]
		public ProjectMemberDTO AddMember([NotEmpty] string project, [NotEmpty] Guid userId, [NotEmpty] string[] roles)
		{
			var mainDao = DaoFactory.CreateMainCrudDao();
			var p = mainDao.Find<ProjectModel>(q => q.Where(m => m.ProjectCode == project));
			if (p == null)
				throw new NoSuchProjectException();

			SecurityService.DemandUpdate(p, p.ProjectCode, CurrentUser.Id, MainSession);
			var u = mainDao.Get<UserModel>(userId, true);

			if (!TaskProjectRoles.IsValid(roles))
				throw new ArgumentException("Not valid role(s)", "roles");

			var projDao = DaoFactory.CreateProjectCrudDao(project);
			if (projDao.Exists<ProjectMemberModel>(q => q.Where(m => m.ProjectCode == p.ProjectCode && m.UserId == u.Id)))
				throw new UserAlreadyProjectMemberException();

			var membership = new ProjectMembershipModel { Project = p, User = u };
			p.Members.Add(membership);
			mainDao.Store(membership);

			var member = ProjectMemberModel.FromParameters(u, p, roles);
			DemandUpdate(member, project);
			projDao.Store(member);
			
			return new ProjectMemberAdapter(_LocalizationService).Fill(member);
		}

		[JsonEndpoint, RequireAuthorization]
		public void RemoveMember([NotEmpty] string project, [NotEmpty] Guid memberId)
		{
			var mainDao = DaoFactory.CreateMainCrudDao();
			var projDao = DaoFactory.CreateProjectCrudDao(project);
			var member = projDao.Get<ProjectMemberModel>(memberId, true);
			
			DemandDelete(member, member.ProjectCode);

			if (projDao.Exists<TaskExecutorModel>(q => q.Where(m => m.Executor.Id == member.Id)) ||
				projDao.Exists<TaskAgreementModel>(q => q.Where(m => m.Agreemer.Id == member.Id)))
				throw new CannotDeleteReferencedItemException();

			var p = mainDao.Find<ProjectModel>(q => q.Where(m => m.ProjectCode == member.ProjectCode));
			var membership = mainDao.Find<ProjectMembershipModel>(q =>
				q.Where(m => m.ProjectId == p.Id && m.User.Id == member.UserId));
			projDao.Delete(member);
			mainDao.Delete(membership);
		}

		private static IDictionary<string, LookupEntry[]> cache;
		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupRoles(string term, int page)
		{
			if (page > 0) return Enumerable.Empty<LookupEntry>(); //while size of roles less than defaul page size (10)

			var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
			if (cache == null)
			{
				//no need to locking - replace with same value from another thread has no negative effect
				cache = new Dictionary<string, LookupEntry[]>();
			}
			if (!cache.ContainsKey(lang))
			{
				//no need to locking - replace with same value from another thread has no negative effect
				cache[lang] = TaskProjectRoles.Roles(_LocalizationService);
			}

			if (term.IsNullOrWhiteSpace())
				return cache[lang];

			return cache[lang]
				.Where(l => l.Text.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0)
				.ToArray();
		}

		[JsonEndpoint, RequireAuthorization]
		public UpdateResult<ProjectMemberDTO> ChangeMemberRoles([NotEmpty] string project, [NotEmpty] Guid memberId, [NotEmpty] string[] roles)
		{
			return Edit<ProjectMemberModel, ProjectMemberDTO>(memberId, project,
			(model, validation) =>
			{
				//TODO localization
				if (!TaskProjectRoles.IsValid(roles))
				{
					validation.AddFieldErrors("Roles", "Roles contains incorrect values");
					return;
				}
				if (!roles.Contains(model.CurrentRole))
				{
					validation.AddFieldErrors("Roles", "Can not change roles. Current member role not exist in provided roles.");
					return;
				}

				model.Roles = roles;
			},
			pmm => new ProjectMemberAdapter(_LocalizationService).Fill(pmm),
			() => { throw new ProjectMemberCreationNotSupportedException(); });
		}

		[JsonEndpoint, RequireAuthorization]
		public UpdateResult<ProjectMemberDTO> ChangeMemberCurrentRole([NotEmpty] string project,  [NotEmpty] Guid memberId, [NotEmpty] string current)
		{
			return Edit<ProjectMemberModel, ProjectMemberDTO>(memberId, project,
			(model, validation) =>
			{
				//TODO localization
				if (!TaskProjectRoles.IsValid(current))
				{
					validation.AddFieldErrors("CurrentRole", "Incorrect role");
					return;
				}
				if (!model.HasRole(current))
				{
					validation.AddFieldErrors("CurrentRole", "Can not change current role. Member does not have this role assigned.");
					return;
				}

				model.CurrentRole = current;
			},
			pmm => new ProjectMemberAdapter(_LocalizationService).Fill(pmm),
			() => { throw new ProjectMemberCreationNotSupportedException(); });
		}


		//TODO needs abstraction in core, that require this method (optional, some modules may not support role switching)
		[JsonEndpoint, RequireAuthorization]
		public bool SwitchRole([NotEmpty] string project, [NotEmpty] string current)
		{
			var p = DaoFactory.CreateMainCrudDao().Find<ProjectModel>(q => q.Where(m => m.ProjectCode == project));
			if (p == null)
				throw new NoSuchProjectException();

			var member = DaoFactory.CreateProjectCrudDao(project).Find<ProjectMemberModel>(q => q.Where(
				m => m.ProjectCode == p.ProjectCode && m.UserId == CurrentUser.Id));
			if (member == null)
				throw new NoSuchProjectMemberException();

			return ChangeMemberCurrentRole(member.ProjectCode, member.Id, current).Validation.Success;
		}
	}
}

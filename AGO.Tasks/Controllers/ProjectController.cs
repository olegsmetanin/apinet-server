using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AGO.Core;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Controllers.Security;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Core.Modules.Attributes;
using AGO.Tasks.Controllers.DTO;
using AGO.Tasks.Model.Task;
using NHibernate;
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
			AuthController authController) : base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, localizationService, modelProcessingService, authController)
		{
		}

		[JsonEndpoint, RequireAuthorization]
		public ProjectDTO GetProject([NotEmpty] string project)
		{
			var p = _CrudDao.Find<ProjectModel>(q => q.Where(m => m.ProjectCode == project));

			if (p == null)
				throw new NoSuchProjectException();

			var adapter = new ProjectAdapter(_LocalizationService, _AuthController.CurrentUser());
			return adapter.Fill(p);
		}

		[JsonEndpoint, RequireAuthorization]
		public UpdateResult<ProjectDTO> UpdateProject([NotEmpty] string project, [NotNull] PropChangeDTO data)
		{
			if (!_CrudDao.Exists<ProjectModel>(q => q.Where(m => m.ProjectCode == project)))
				throw new NoSuchProjectException();

			var user = _AuthController.CurrentUser();
			return Edit<ProjectModel, ProjectDTO>(data.Id, project,
				(p, vr) =>
				{
					if (data.Prop.IsNullOrWhiteSpace())
					{
						vr.AddErrors("Property name required");
						return;
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
								var isAdmin = _CrudDao.Exists<ProjectMemberModel>(q => q.Where(
									m => m.ProjectCode == project && m.UserId == user.Id && m.CurrentRole == BaseProjectRoles.Administrator));
								if (!isAdmin)
									throw new AccessForbiddenException();

								p.VisibleForAll = data.Value.ConvertSafe<bool>();
								break;
							case "Status":
								//TODO business logic, security and other checks
								var newStatus = data.Value.ConvertSafe<ProjectStatus>();
								p.ChangeStatus(newStatus, _AuthController.CurrentUser());//create entity saved via cascade
								break;
							default:
								vr.AddErrors(string.Format("Unsupported prop for update: '{0}'", data.Prop));
								break;
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

				},
				p => new ProjectAdapter(_LocalizationService, user).Fill(p),
				() => { throw new ProjectCreationNotSupportedException(); });
		}

		[JsonEndpoint, RequireAuthorization]
		public bool TagProject(
			[NotEmpty] Guid modelId,
			[NotEmpty] Guid tagId)
		{
			var currentUser = _AuthController.CurrentUser();

			var projectToTag = _SessionProvider.CurrentSession.QueryOver<ProjectToTagModel>()
				.Where(m => m.Project.Id == modelId && m.Tag.Id == tagId).SingleOrDefault();

			if (projectToTag != null)
				return false;

			var project = _CrudDao.Get<ProjectModel>(modelId, true);
			if ((project.Creator == null || !currentUser.Equals(project.Creator)) && currentUser.SystemRole != SystemRole.Administrator)
				throw new AccessForbiddenException();

			var tag = _CrudDao.Get<ProjectTagModel>(tagId, true);
			if ((tag.Creator == null || !currentUser.Equals(tag.Creator)) && currentUser.SystemRole != SystemRole.Administrator)
				throw new AccessForbiddenException();

			_CrudDao.Store(new ProjectToTagModel
			{
				Creator = currentUser,
				Project = project,
				Tag = tag
			});

			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public bool DetagProject(
			[NotEmpty] Guid modelId,
			[NotEmpty] Guid tagId)
		{
			var currentUser = _AuthController.CurrentUser();

			var projectToTag = _SessionProvider.CurrentSession.QueryOver<ProjectToTagModel>()
				.Where(m => m.Project.Id == modelId && m.Tag.Id == tagId).SingleOrDefault();

			if (projectToTag == null)
				return false;

			var project = projectToTag.Project;
			if ((project.Creator == null || !currentUser.Equals(project.Creator)) && currentUser.SystemRole != SystemRole.Administrator)
				throw new AccessForbiddenException();

			var tag = projectToTag.Tag;
			if ((tag.Creator == null || !currentUser.Equals(tag.Creator)) && currentUser.SystemRole != SystemRole.Administrator)
				throw new AccessForbiddenException();

			_CrudDao.Delete(projectToTag);

			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<ProjectMemberDTO> GetMembers([NotEmpty] string project, string term, [InRange(0, null)] int page)
		{
			var q = MakeMembersPredicate(project, term);
			var adapter = new ProjectMemberAdapter(_LocalizationService);
			return _CrudDao.PagedQuery(q, page).List().Select(adapter.Fill).ToList();
		}

		[JsonEndpoint, RequireAuthorization]
		public int GetMembersCount([NotEmpty] string project, string term)
		{
			var q = MakeMembersPredicate(project, term);
			return q.RowCount();
		}

		private IQueryOver<ProjectMemberModel> MakeMembersPredicate(string project, string term)
		{
			var q = Session.QueryOver<ProjectMemberModel>()
				.Where(m => m.ProjectCode == project)
				.OrderBy(m => m.FullName).Asc;
			if (!term.IsNullOrWhiteSpace())
				q = q.WhereRestrictionOn(m => m.FullName).IsLike(term.TrimSafe(), MatchMode.Anywhere);

			return q;
		}
			
		[JsonEndpoint, RequireAuthorization]
		public ProjectMemberDTO AddMember([NotEmpty] string project, [NotEmpty] Guid userId, [NotEmpty] string[] roles)
		{
			var p = _CrudDao.Find<ProjectModel>(q => q.Where(m => m.ProjectCode == project));
			if (p == null)
				throw new NoSuchProjectException();
			var u = _CrudDao.Get<UserModel>(userId, true);

			if (!TaskProjectRoles.IsValid(roles))
				throw new ArgumentException("Not valid role(s)", "roles");

			if (_CrudDao.Exists<ProjectMemberModel>(q => q.Where(m => m.ProjectCode == p.ProjectCode && m.UserId == u.Id)))
				throw new UserAlreadyProjectMemberException();

			var member = ProjectMemberModel.FromParameters(u, p, roles);
			_CrudDao.Store(member);
			return new ProjectMemberAdapter(_LocalizationService).Fill(member);
		}

		[JsonEndpoint, RequireAuthorization]
		public void RemoveMember([NotEmpty] Guid memberId)
		{
			var member = _CrudDao.Get<ProjectMemberModel>(memberId, true);
			//TODO security checks
			
			if (_CrudDao.Exists<TaskExecutorModel>(q => q.Where(m => m.Executor.Id == member.Id)) ||
				_CrudDao.Exists<TaskAgreementModel>(q => q.Where(m => m.Agreemer.Id == member.Id)))
				throw new CannotDeleteReferencedItemException();

			_CrudDao.Delete(member);
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
		public UpdateResult<ProjectMemberDTO> ChangeMemberRoles([NotEmpty] Guid memberId, [NotEmpty] string[] roles)
		{
			var member = _CrudDao.Get<ProjectMemberModel>(memberId, true);
			

			//TODO only project admins may change this

			return Edit<ProjectMemberModel, ProjectMemberDTO>(member.Id, member.ProjectCode,
			(model, validation) =>
			{
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
		public UpdateResult<ProjectMemberDTO> ChangeMemberCurrentRole([NotEmpty] Guid memberId, [NotEmpty] string current)
		{
			var member = _CrudDao.Get<ProjectMemberModel>(memberId, true);


			//TODO only project admins or user themself may change this

			return Edit<ProjectMemberModel, ProjectMemberDTO>(member.Id, member.ProjectCode,
			(model, validation) =>
			{
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


		//TODO needs abstraction in core, that require this method (optional, some modules may does not support role switching)
		[JsonEndpoint, RequireAuthorization]
		public bool SwitchRole([NotEmpty] string project, [NotEmpty] string current)
		{
			var p = _CrudDao.Find<ProjectModel>(q => q.Where(m => m.ProjectCode == project));
			if (p == null)
				throw new NoSuchProjectException();
			var user = _AuthController.CurrentUser();
			var member = _CrudDao.Find<ProjectMemberModel>(q => q.Where(m => m.ProjectCode == p.ProjectCode && m.UserId == user.Id));
			if (member == null)
				throw new NoSuchProjectMemberException();

			return ChangeMemberCurrentRole(member.Id, current).Validation.Success;
		}
	}
}

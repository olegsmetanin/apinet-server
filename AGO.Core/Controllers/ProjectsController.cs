using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AGO.Core.Application;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers.Projects;
using AGO.Core.Controllers.Security;
using AGO.Core.DataAccess;
using AGO.Core.Filters.Metadata;
using AGO.Core.Localization;
using AGO.Core.Model.Configuration;
using AGO.Core.Model.Security;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Projects;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Modules.Attributes;
using AGO.Core.Security;
using NHibernate.Criterion;
using Npgsql;


namespace AGO.Core.Controllers
{
	public enum ProjectsRequestMode
	{
		All,
		Participated
	}

	public class ProjectsController : AbstractController
	{
		#region Properties, fields, constructors

		private readonly IPersistenceApplication app;
		private readonly IProjectFactory[] projFactories;

		public ProjectsController(
			IJsonService jsonService,
			IFilteringService filteringService,
			ILocalizationService localizationService,
			IModelProcessingService modelProcessingService,
			AuthController authController,
			ISecurityService securityService,
			ISessionProviderRegistry registry,
			DaoFactory factory,
			IEnumerable<IProjectFactory> projFactories)
			: base(jsonService, filteringService, localizationService, modelProcessingService, authController, securityService, registry, factory)
		{
			app = AbstractApplication.Current as IPersistenceApplication;
			Debug.Assert(app != null, "Can't grab current persistent application");
			this.projFactories = (projFactories ?? Enumerable.Empty<IProjectFactory>()).ToArray();
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization]
		public bool TagProject(
			[NotEmpty] Guid modelId,
			[NotEmpty] Guid tagId)
		{
			var currentUser = _AuthController.CurrentUser();

			var projectToTag = MainSession.QueryOver<ProjectToTagModel>()
				.Where(m => m.Project.Id == modelId && m.Tag.Id == tagId).Take(1).SingleOrDefault();

			if (projectToTag != null)
				return false;

			var dao = DaoFactory.CreateMainCrudDao();
			var project = dao.Get<ProjectModel>(modelId, true);
			var tag = dao.Get<ProjectTagModel>(tagId, true);
			var link = new ProjectToTagModel
			{
				Project = project,
				Tag = tag
			};
			SecurityService.DemandUpdate(link, project.ProjectCode, currentUser.Id, MainSession);
			dao.Store(link);

			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public bool DetagProject(
			[NotEmpty] Guid modelId, 
			[NotEmpty] Guid tagId)
		{
			var currentUser = _AuthController.CurrentUser();

			var projectToTag = MainSession.QueryOver<ProjectToTagModel>()
				.Where(m => m.Project.Id == modelId && m.Tag.Id == tagId).Take(1).SingleOrDefault();

			if (projectToTag == null)
				return false;

			SecurityService.DemandDelete(projectToTag, projectToTag.Project.ProjectCode, currentUser.Id, MainSession);
			DaoFactory.CreateMainCrudDao().Delete(projectToTag);

			return true;
		}

		[JsonEndpoint, RequireAuthorization(true)]
		public IEnumerable<LookupEntry> LookupDbInstances(string term, [InRange(0, null)] int page)
		{
			var query = MainSession.QueryOver<DbInstanceModel>()
				.OrderBy(m => m.Name).Asc;
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.Name).IsLike(term.TrimSafe(), MatchMode.Anywhere);

			return DaoFactory.CreateMainCrudDao().PagedQuery(query, page).LookupModelsList(m => m.Name);
		}
			
		[JsonEndpoint, RequireAuthorization(true)]
		public object CreateProject([NotNull] ProjectModel model, [NotEmpty] Guid serverId, [NotNull] ISet<Guid> tagIds, bool skipDbCreation = false)
		{
			var validation = new ValidationResult();

			try
			{
				var dao = DaoFactory.CreateMainCrudDao();
				var newProject = new ProjectModel
				{
					CreationTime = DateTime.UtcNow,
					ProjectCode = model.ProjectCode.TrimSafe(),
					Name = model.Name.TrimSafe(),
					Description = model.Description.TrimSafe(),
					Type = dao.Get<ProjectTypeModel>(model.TypeId, true),
					VisibleForAll = model.VisibleForAll
				};
				newProject.ChangeStatus(ProjectStatus.New, CurrentUser);

				if (!skipDbCreation)
				{
					var dbinstance = dao.Get<DbInstanceModel>(serverId, true);
					newProject.ConnectionString = BuildProjectConnectionString(newProject, dbinstance,
						MainSession.Connection.ConnectionString);
				}
				else
				{
					newProject.ConnectionString = MainSession.Connection.ConnectionString;
				}

				_ModelProcessingService.ValidateModelSaving(newProject, validation, MainSession);
				if (!validation.Success)
					return validation;

				//need to call before first Store called, because after this IsNew return false
				SecurityService.DemandUpdate(newProject, newProject.ProjectCode, CurrentUser.Id, MainSession);

				dao.Store(newProject);
				MainSession.Flush();//next code find project in db

				//Make project database
				if (!skipDbCreation)
					app.CreateProjectDatabase(newProject.ConnectionString, newProject.Type.Module);

				//make current user project admin and do next secure logged things as of this member
				var membership = new ProjectMembershipModel { Project = newProject, User = CurrentUser };
				newProject.Members.Add(membership);
				dao.Store(newProject);
				MainSession.Flush();//required for ProjectSession() working

				var member = ProjectMemberModel.FromParameters(CurrentUser, newProject, BaseProjectRoles.Administrator);
				DaoFactory.CreateProjectCrudDao(newProject.ProjectCode).Store(member);
				ProjectSession(newProject.ProjectCode).Flush();

				_ModelProcessingService.AfterModelCreated(newProject, member);

				foreach (var tag in tagIds.Select(id => dao.Get<ProjectTagModel>(id)))
				{
					dao.Store(new ProjectToTagModel
					{
						Tag = tag,
						Project = newProject
					});
				}

				var projFactory = projFactories.FirstOrDefault(f => f.Accept(newProject));
				if (projFactory != null)
				{
					projFactory.Handle(newProject);
				}

				return newProject;
			}
			catch (Exception e)
			{
				validation.AddErrors(_LocalizationService.MessageForException(e));
				return validation;
			}
		}

		private static string BuildProjectConnectionString(ProjectModel project, DbInstanceModel db, string template)
		{
			if (project == null)
				throw new ArgumentNullException("project");
			if (db == null)
				throw new ArgumentNullException("db");

			var builder = new NpgsqlConnectionStringBuilder(template);
			builder.Host = db.Server;
			builder.Database = CalculateProjectDbName(project.ProjectCode);

			return builder.ConnectionString;
		}

		private static string CalculateProjectDbName(string project)
		{
			if (project.IsNullOrWhiteSpace())
				throw new ArgumentNullException("project");

			return "ago_" + project;
		}

		private IEnumerable<IModelFilterNode> MakeProjectsPredicate(
			ICollection<IModelFilterNode> filter,
			ProjectsRequestMode mode)
		{
			if (mode == ProjectsRequestMode.Participated)
			{
				var membershipFilter = _FilteringService.Filter<ProjectModel>()
					.WhereCollection(m => m.Members).Where(m => m.User.Id == CurrentUser.Id);

				filter.Add(membershipFilter);
			}
			return new [] { SecurityService.ApplyReadConstraint<ProjectModel>(null, CurrentUser.Id, MainSession, filter.ToArray())};
		}
			
		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<ProjectViewModel> GetProjects(
			[InRange(0, null)] int page,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters,
			ProjectsRequestMode mode)
		{
			var projects = DaoFactory.CreateMainFilteringDao()
				.List<ProjectModel>(MakeProjectsPredicate(filter, mode), new FilteringOptions
			{
				Page = page,
				Sorters = sorters
			});

			return projects.Select(project =>
			{
				var viewModel = new ProjectViewModel(project);

				var allowed = project.Tags.Where(m => m.Tag.OwnerId == CurrentUser.Id);
				viewModel.Tags.UnionWith(allowed.OrderBy(tl => tl.Tag.FullName).Select(m => new LookupEntry
				{
					Id = m.Tag.Id.ToString(),
					Text = m.Tag.FullName
				}));

				return viewModel;
			}).ToList();
		}

		[JsonEndpoint, RequireAuthorization]
		public int GetProjectsCount([NotNull] ICollection<IModelFilterNode> filter, ProjectsRequestMode mode)
		{
			return DaoFactory.CreateMainFilteringDao().RowCount<ProjectModel>(MakeProjectsPredicate(filter, mode));
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupProjects(string term, [InRange(0, null)] int page)
		{
			var fb = _FilteringService.Filter<ProjectModel>();
			IModelFilterNode termFilter = null;
			if (!term.IsNullOrWhiteSpace())
			{
				termFilter = fb.Or()
					.WhereString(m => m.ProjectCode).Like(term.TrimSafe(), true, true)
					.WhereString(m => m.Name).Like(term.TrimSafe(), true, true);
			}
			var filter = SecurityService.ApplyReadConstraint<ProjectModel>(null, CurrentUser.Id, MainSession, termFilter);
			var criteria = _FilteringService.CompileFilter(filter, typeof (ProjectModel)).GetExecutableCriteria(MainSession);
			criteria.AddOrder(Order.Asc(Projections.Property<ProjectModel>(m => m.Name).PropertyName));

			return DaoFactory.CreateMainCrudDao().PagedCriteria(criteria, page).LookupList<ProjectModel>(m => m.ProjectCode, m => m.Name);
		}
			
		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupParticipant(
			[NotEmpty] string project,
			string term,
			[InRange(0, null)] int page)
		{
			var query = ProjectSession(project).QueryOver<ProjectMemberModel>()
				.Where(m => m.ProjectCode == project)
				.OrderBy(m => m.FullName).Asc;
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.FullName).IsLike(term.TrimSafe(), MatchMode.Anywhere);

			return DaoFactory.CreateProjectCrudDao(project).PagedQuery(query, page).LookupModelsList(m => m.FullName);
		}
			
		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> ProjectMetadata()
		{
			return MetadataForModelAndRelations<ProjectModel>();
		}

		[JsonEndpoint, RequireAuthorization]
		public object ProjectInfo([NotEmpty] string project)
		{
			var projectModel = MainSession.QueryOver<ProjectModel>()
			    .Where(m => m.ProjectCode == project)
			    .Take(1).SingleOrDefault();
			if (projectModel == null || projectModel.Type == null)
				throw new NoSuchProjectException();

			return new
			{
				projectModel.Type.Module,
				projectModel.Name
			};
		}

		#endregion
	}
}
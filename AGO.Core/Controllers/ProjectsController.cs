using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers.Projects;
using AGO.Core.Controllers.Security;
using AGO.Core.Filters.Metadata;
using AGO.Core.Localization;
using AGO.Core.Model.Security;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Projects;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Modules.Attributes;
using NHibernate.Criterion;

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

		public ProjectsController(
			IJsonService jsonService,
			IFilteringService filteringService,
			ICrudDao crudDao,
			IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			ILocalizationService localizationService,
			IModelProcessingService modelProcessingService,
			AuthController authController)
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, localizationService, modelProcessingService, authController)
		{
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization]
		public bool TagProject(
			[NotEmpty] Guid modelId,
			[NotEmpty] Guid tagId)
		{
			var currentUser = _AuthController.CurrentUser();

			var projectToTag = _SessionProvider.CurrentSession.QueryOver<ProjectToTagModel>()
				.Where(m => m.Project.Id == modelId && m.Tag.Id == tagId).Take(1).SingleOrDefault();

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
				.Where(m => m.Project.Id == modelId && m.Tag.Id == tagId).Take(1).SingleOrDefault();

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

		[JsonEndpoint, RequireAuthorization(true)]
		public object CreateProject([NotNull] ProjectModel model, [NotNull] ISet<Guid> tagIds)
		{
			var validation = new ValidationResult();

			try
			{
				var newProject = new ProjectModel
				{
					Creator = _AuthController.CurrentUser(),
					ProjectCode = model.ProjectCode.TrimSafe(),
					Name = model.Name.TrimSafe(),
					Description = model.Description.TrimSafe(),
					Type = model.TypeId != null && !default(Guid).Equals(model.TypeId)
						? _CrudDao.Get<ProjectTypeModel>(model.TypeId)
						: null,
					VisibleForAll = model.VisibleForAll,
					Status = ProjectStatus.New,
				};

				_ModelProcessingService.ValidateModelSaving(newProject, validation);
				if (!validation.Success)
					return validation;

				_CrudDao.Store(newProject);

				var statusHistoryRow = newProject.ChangeStatus(ProjectStatus.New, _AuthController.CurrentUser());
				_CrudDao.Store(statusHistoryRow);

				foreach (var tag in tagIds.Select(id => _CrudDao.Get<ProjectTagModel>(id)))
				{
					_CrudDao.Store(new ProjectToTagModel
					{
						Creator = _AuthController.CurrentUser(),
						Tag = tag,
						Project = newProject
					});
				}

				_CrudDao.Store(ProjectMemberModel.FromParameters(newProject.Creator, newProject, ProjectModel.ADMIN_GROUP));

				return newProject;
			}
			catch (Exception e)
			{
				validation.AddErrors(_LocalizationService.MessageForException(e));
				return validation;
			}
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<ProjectViewModel> GetProjects(
			[InRange(0, null)] int page,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters,
			ProjectsRequestMode mode)
		{
			if (mode == ProjectsRequestMode.Participated)
			{
				var modeFilter = new ModelFilterNode { Path = "Participants" };
				modeFilter.AddItem(new ValueFilterNode
				{
					Path = "User",
					Operator = ValueFilterOperators.Eq,
					Operand = _AuthController.CurrentUser().Id.ToString()
				});
				filter.Add(modeFilter);
			}
			
			var projects = _FilteringDao.List<ProjectModel>(filter, new FilteringOptions
			{
				Page = page,
				Sorters = sorters
			});

			return projects.Select(project =>
			{
				var viewModel = new ProjectViewModel(project);

				var allowed = project.Tags.Where(m => m.Tag.CreatorId != null && m.Tag.CreatorId == _AuthController.CurrentUser().Id);
				viewModel.Tags.UnionWith(allowed.OrderBy(tl => tl.Tag.Creator).ThenBy(tl => tl.Tag.FullName).Select(m => new LookupEntry
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
			if (mode == ProjectsRequestMode.Participated)
			{
				var modeFilter = new ModelFilterNode { Path = "Participants" };
				modeFilter.AddItem(new ValueFilterNode
				{
					Path = "User",
					Operator = ValueFilterOperators.Eq,
					Operand = _AuthController.CurrentUser().Id.ToString()
				});
				filter.Add(modeFilter);
			}

			return _FilteringDao.RowCount<ProjectModel>(filter);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupParticipant(
			[NotEmpty] string project,
			string term,
			[InRange(0, null)] int page)
		{
			var query = _SessionProvider.CurrentSession.QueryOver<ProjectMemberModel>()
				.Where(m => m.ProjectCode == project)
				.OrderBy(m => m.FullName).Asc;
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.FullName).IsLike(term.TrimSafe(), MatchMode.Anywhere);

			return _CrudDao.PagedQuery(query, page).LookupModelsList(m => m.FullName);
		}
			
		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> ProjectMetadata()
		{
			return MetadataForModelAndRelations<ProjectModel>();
		}

		[JsonEndpoint, RequireAuthorization]
		public object ProjectInfo([NotEmpty] string project)
		{
			var projectModel = _SessionProvider.CurrentSession.QueryOver<ProjectModel>()
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
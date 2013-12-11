using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Filters.Metadata;
using AGO.Core.Localization;
using AGO.Core.Model.Security;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Projects;
using AGO.Core;
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

		[JsonEndpoint, RequireAuthorization(true)]
		public object CreateProject([NotNull] ProjectModel model)
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
					Status = ProjectStatus.New,
				};

				_ModelProcessingService.ValidateModelSaving(newProject, validation);
				if (!validation.Success)
					return validation;

				_CrudDao.Store(newProject);

				var statusHistoryRow = new ProjectStatusHistoryModel
				{
					StartDate = DateTime.Now,
					Project = newProject,
					Status = ProjectStatus.New
				};
				_CrudDao.Store(statusHistoryRow);

				return newProject;
			}
			catch (Exception e)
			{
				validation.AddErrors(_LocalizationService.MessageForException(e));
				return validation;
			}
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<ProjectModel> GetProjects(
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

			return _FilteringDao.List<ProjectModel>(filter, new FilteringOptions
			{
				Page = page,
				Sorters = sorters
			});
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
//			var filter = _FilteringService.Filter<ProjectParticipantModel>()
//				.And()
//				.Where(m => m.Project.ProjectCode == project);
//			if (!term.IsNullOrWhiteSpace())
//				filter = filter.WhereString(m => m.User.FullName).Like("%" + term.TrimSafe() + "%");
//
//			var criteria = _FilteringService
//				.CompileFilter(filter, typeof (ProjectParticipantModel))
//				.AddOrder(Order.Asc(Projections.Property<ProjectParticipantModel>(m => m.User.FullName)))
//				.SetResultTransformer(Transformers.AliasToBean(typeof (LookupEntry)));

			ProjectParticipantModel ppm = null;
			ProjectModel pm = null;
			UserModel um = null;
			var query = _SessionProvider.CurrentSession.QueryOver(() => ppm)
				.JoinAlias(() => ppm.Project, () => pm)
				.JoinAlias(() => ppm.User, () => um)
				.Where(() => pm.ProjectCode == project)
				.OrderBy(() =>  um.FullName).Asc;
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(() => um.FullName).IsLike(term.TrimSafe(), MatchMode.Anywhere);

			return _CrudDao.PagedQuery(query, page)
				.LookupList<ProjectParticipantModel, UserModel>(m => m.Id, "um", u => u.FullName).ToArray();
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

			return new { projectModel.Type.Module };
		}

		#endregion
	}
}
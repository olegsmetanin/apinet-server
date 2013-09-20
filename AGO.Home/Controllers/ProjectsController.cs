using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Filters.Metadata;
using AGO.Home.Model.Dictionary.Projects;
using AGO.Home.Model.Projects;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Modules.Attributes;
using NHibernate.Criterion;

namespace AGO.Home.Controllers
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
			AuthController authController)
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, authController)
		{
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization(true)]
		public ValidationResult CreateProject([NotNull] ProjectModel model)
		{
			var validationResult = new ValidationResult();

			try
			{
				var initialStatus = _SessionProvider.CurrentSession.QueryOver<ProjectStatusModel>()
					.Where(m => m.IsInitial).Take(1).List().FirstOrDefault();
				if (initialStatus == null)
					throw new NoInitialProjectStatusException();

				var projectType = model.TypeId != null && !default(Guid).Equals(model.TypeId)
					? _CrudDao.Get<ProjectTypeModel>(model.TypeId)
					: null;
				if (projectType == null)
					validationResult.FieldErrors["Type"] = new RequiredFieldException().Message;

				var projectCode = model.ProjectCode.TrimSafe();
				if (projectCode.IsNullOrEmpty())
					validationResult.FieldErrors["ProjectCode"] = new RequiredFieldException().Message;
				if (_SessionProvider.CurrentSession.QueryOver<ProjectModel>()
						.Where(m => m.ProjectCode == projectCode).RowCount() > 0)
					validationResult.FieldErrors["ProjectCode"] = new UniqueFieldException().Message;

				var name = model.Name.TrimSafe();
				if (name.IsNullOrEmpty())
					validationResult.FieldErrors["Name"] = new RequiredFieldException().Message;

				if (!validationResult.Success)
					return validationResult;
				
				var newProject = new ProjectModel
				{
					Creator = _AuthController.CurrentUser(),
					ProjectCode = projectCode,
					Name = name,
					Description = model.Description.TrimSafe(),
					Type = projectType,
					Status = initialStatus,
				};
				_CrudDao.Store(newProject);

				var statusHistoryRow = new ProjectStatusHistoryModel
				{
					StartDate = DateTime.Now,
					Project = newProject,
					Status = initialStatus
				};
				_CrudDao.Store(statusHistoryRow);
			}
			catch (Exception e)
			{
				validationResult.GeneralError = e.Message;
			}
			
			return validationResult;
		}

		/*[JsonEndpoint, RequireAuthorization(true)]
		public void DeleteProject(JsonReader input)
		{
			var request = _JsonRequestService.ParseModelRequest<Guid>(input);

			var project = _CrudDao.Get<ProjectModel>(request.Id, true);
			_CrudDao.Delete(project);
		}*/

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<ProjectModel> GetProjects(
			[InRange(0, null)] int page,
			[InRange(0, MaxPageSize)] int pageSize,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters,
			ProjectsRequestMode mode)
		{
			pageSize = pageSize == 0 ? DefaultPageSize : pageSize;

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
				Skip = page * pageSize,
				Take = pageSize,
				Sorters = sorters
			});
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupProjectNames(
			[InRange(0, null)] int page,
			[InRange(0, MaxPageSize)] int pageSize,
			string term)
		{
			pageSize = pageSize == 0 ? DefaultPageSize : pageSize;

			var query = _SessionProvider.CurrentSession.QueryOver<ProjectModel>()
				.Select(Projections.Distinct(Projections.Property("Name")))
				.OrderBy(m => m.Name).Asc;
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.Name).IsLike(term.TrimSafe(), MatchMode.Anywhere);

			return query.Skip(page*pageSize).Take(pageSize).LookupList(m => m.Name);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> ProjectMetadata()
		{
			return MetadataForModelAndRelations<ProjectModel>();
		}

		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AGO.Home.Controllers
{
	public enum ProjectsRequestMode
	{
		All,
		Participated
	}

	public class ProjectsController : AbstractController
	{
		#region Constants

		public const string NewProjectModelName = "project";

		public const string ProjectsRequestModeName = "mode";

		#endregion

		#region Properties, fields, constructors

		public ProjectsController(
			IJsonService jsonService,
			IFilteringService filteringService,
			IJsonRequestService jsonRequestService,
			ICrudDao crudDao,
			IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			AuthController authController)
			: base(jsonService, filteringService, jsonRequestService, crudDao, filteringDao, sessionProvider, authController)
		{
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization(true)]
		public ValidationResult CreateProject(JsonReader input)
		{
			var request = _JsonRequestService.ParseRequest(input);
			var validationResult = new ValidationResult();

			try
			{
				var modelProperty = request.Body.Property(NewProjectModelName);
				if (modelProperty == null)
					throw new MalformedRequestException();

				var model = _JsonService.CreateSerializer().Deserialize<ProjectModel>(
					new JTokenReader(modelProperty.Value));
				if (model == null)
					throw new MalformedRequestException();

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
					Creator = _AuthController.GetCurrentUser(),
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
		public object GetProjects(JsonReader input)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			var modeProperty = request.Body.Property(ProjectsRequestModeName);
			var mode = (modeProperty != null ? modeProperty.TokenValue() : string.Empty)
				.ParseEnumSafe(ProjectsRequestMode.All);
	
			if (mode == ProjectsRequestMode.Participated)
			{
				var modeFilter = new ModelFilterNode { Path = "Participants" };
				modeFilter.AddItem(new ValueFilterNode
				{
					Path = "User",
					Operator = ValueFilterOperators.Eq,
					Operand = _AuthController.GetCurrentUser().Id.ToString()
				});
				request.Filters.Add(modeFilter);
			}

			return new
			{
				totalRowsCount = _FilteringDao.RowCount<ProjectModel>(request.Filters),
				rows = _FilteringDao.List<ProjectModel>(request.Filters, OptionsFromRequest(request))
			};
		}

		[JsonEndpoint, RequireAuthorization]
		public object LookupProjectNames(JsonReader input)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			var termProperty = request.Body.Property("term");
			var term = termProperty != null ? termProperty.TokenValue() : null;
			var options = OptionsFromRequest(request);

			var query = _SessionProvider.CurrentSession.QueryOver<ProjectModel>()
				.Select(Projections.Distinct(Projections.Property("Name")));
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.Name).IsLike(term, MatchMode.Anywhere);

			var result = new List<object>();

			foreach (var str in query.Skip(options.Skip ?? 0).Take(options.Take ?? 1).List<string>())
				result.Add(new { id = str, text = str });

			return new { rows = result };
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> ProjectMetadata(JsonReader input)
		{
			return MetadataForModelAndRelations<ProjectModel>();
		}

		#endregion
	}
}
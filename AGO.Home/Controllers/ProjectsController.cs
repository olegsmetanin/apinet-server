using System;
using System.Linq;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Home.Model.Dictionary.Projects;
using AGO.Home.Model.Projects;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Modules.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AGO.Home.Controllers
{
	public class ProjectsController : AbstractController
	{
		#region Constants

		public const string NewProjectModelName = "project";

		#endregion

		#region Properties, fields, constructors

		protected readonly AuthController _AuthController;

		public ProjectsController(
			IJsonService jsonService,
			IFilteringService filteringService,
			IJsonRequestService jsonRequestService,
			ICrudDao crudDao,
			IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			AuthController authController)
			: base(jsonService, filteringService, jsonRequestService, crudDao, filteringDao, sessionProvider)
		{
			if (authController == null)
				throw new ArgumentNullException("authController");
			_AuthController = authController;
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization(true)]
		public void CreateProject(JsonReader input, JsonWriter output)
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
					return;
				
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
			finally
			{
				_JsonService.CreateSerializer().Serialize(output, validationResult); 
			}	
		}

		/*[JsonEndpoint, RequireAuthorization(true)]
		public void DeleteProject(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelRequest<Guid>(input);

			var project = _CrudDao.Get<ProjectModel>(request.Id, true);
			_CrudDao.Delete(project);
		}*/

		[JsonEndpoint, RequireAuthorization]
		public void GetProjects(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			_JsonService.CreateSerializer().Serialize(output, new
			{
				totalRowsCount = _FilteringDao.RowCount<ProjectModel>(request.Filters),
				rows = _FilteringDao.List<ProjectModel>(request.Filters, OptionsFromRequest(request))
			});
		}

		[JsonEndpoint, RequireAuthorization]
		public void ProjectMetadata(JsonReader input, JsonWriter output)
		{
			_JsonService.CreateSerializer().Serialize(
				output, MetadataForModelAndRelations<ProjectModel>());
		}

		#endregion
	}
}
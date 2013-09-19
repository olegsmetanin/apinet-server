using System;
using AGO.Core;
using AGO.Core.Controllers;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AGO.Tasks.Controllers
{
    /// <summary>
    /// Контроллер справочников модуля задач
    /// </summary>
    public class DictionaryController: AbstractController
    {
        public DictionaryController(
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

		public ModelsResponse<TaskTypeModel> GetTaskTypes(JsonReader input)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			return new ModelsResponse<TaskTypeModel>
			{
				totalRowsCount = _FilteringDao.RowCount<TaskTypeModel>(request.Filters),
				rows = _FilteringDao.List<TaskTypeModel>(request.Filters, OptionsFromRequest(request))
			};
		}

		public ValidationResult EditTaskType(JsonReader input)
		{
			var request = _JsonRequestService.ParseRequest(input);
			var validationResult = new ValidationResult();

			try
			{
				var modelProperty = request.Body.Property(ModelName);
				if (modelProperty == null)
					throw new MalformedRequestException();

				var requestModel = _JsonService.CreateSerializer().Deserialize<TaskTypeModel>(
					new JTokenReader(modelProperty.Value));
				if (requestModel == null)
					throw new MalformedRequestException();

				var model = default(Guid).Equals(requestModel.Id)
					? new TaskTypeModel { ProjectCode = request.Project }//TODO improve AuthController for work without http context { Creator = authController.GetCurrentUser() }
					: _CrudDao.Get<TaskTypeModel>(requestModel.Id, true);

				var name = requestModel.Name.TrimSafe();
				if (name.IsNullOrEmpty())
					validationResult.FieldErrors["Name"] = new RequiredFieldException().Message;
				if (_SessionProvider.CurrentSession.QueryOver<TaskTypeModel>()
						.Where(m => m.ProjectCode == request.Project && m.Name == name && m.Id != requestModel.Id).RowCount() > 0)
					validationResult.FieldErrors["Name"] = new UniqueFieldException().Message;

				if (!validationResult.Success)
					return validationResult;

				model.Name = name;
				_CrudDao.Store(model);
			}
			catch (Exception e)
			{
				validationResult.GeneralError = e.Message;
			}

			return validationResult;
		}

		public void DeleteTaskType(JsonReader input)
		{
			var request = _JsonRequestService.ParseModelRequest<Guid>(input);

			var taskType = _CrudDao.Get<TaskTypeModel>(request.Id, true);

			if (_SessionProvider.CurrentSession.QueryOver<TaskModel>()
					.Where(m => m.TaskType == taskType).RowCount() > 0)
				throw new CannotDeleteReferencedItemException();

			_CrudDao.Delete(taskType);
		}
    }
}
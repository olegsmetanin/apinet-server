using System;
using System.Collections.Generic;
using AGO.Core;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Controllers;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;

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
            ICrudDao crudDao, 
            IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			AuthController authController)
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, authController)
		{
		}

		public ModelsResponse<TaskTypeModel> GetTaskTypes(
			[InRange(0, null)] int page,
			[InRange(0, MaxPageSize)] int pageSize,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters)
		{
			pageSize = pageSize == 0 ? DefaultPageSize : pageSize;

			return new ModelsResponse<TaskTypeModel>
			{
				totalRowsCount = _FilteringDao.RowCount<TaskTypeModel>(filter),
				rows = _FilteringDao.List<TaskTypeModel>(filter, new FilteringOptions
				{
					Skip = page * pageSize,
					Take = pageSize,
					Sorters = sorters
				})
			};
		}

		public ValidationResult EditTaskType([NotNull] TaskTypeModel model, [NotEmpty] string project)
		{
			var validationResult = new ValidationResult();

			try
			{
				var persistentModel = default(Guid).Equals(model.Id)
					? new TaskTypeModel { ProjectCode = project }//TODO improve AuthController for work without http context { Creator = authController.GetCurrentUser() }
					: _CrudDao.Get<TaskTypeModel>(model.Id, true);

				var name = model.Name.TrimSafe();
				if (name.IsNullOrEmpty())
					validationResult.FieldErrors["Name"] = new RequiredFieldException().Message;
				if (_SessionProvider.CurrentSession.QueryOver<TaskTypeModel>()
						.Where(m => m.ProjectCode == project && m.Name == name && m.Id != model.Id).RowCount() > 0)
					validationResult.FieldErrors["Name"] = new UniqueFieldException().Message;

				if (!validationResult.Success)
					return validationResult;

				persistentModel.Name = name;
				_CrudDao.Store(persistentModel);
			}
			catch (Exception e)
			{
				validationResult.GeneralError = e.Message;
			}

			return validationResult;
		}

		public void DeleteTaskType([NotEmpty] Guid id)
		{
			var taskType = _CrudDao.Get<TaskTypeModel>(id, true);

			if (_SessionProvider.CurrentSession.QueryOver<TaskModel>()
					.Where(m => m.TaskType == taskType).RowCount() > 0)
				throw new CannotDeleteReferencedItemException();

			_CrudDao.Delete(taskType);
		}
    }
}
using System;
using System.Collections.Generic;
using AGO.Core;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Filters;
using AGO.Core.Filters.Metadata;
using AGO.Core.Json;
using AGO.Core.Modules.Attributes;
using AGO.Tasks.Controllers.DTO;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using System.Linq;

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

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<TaskTypeDTO> GetTaskTypes(
			[NotEmpty] string project,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters, 
			[InRange(0, null)] int page,
			[InRange(0, MaxPageSize)] int pageSize)
		{
			pageSize = pageSize == 0 ? DefaultPageSize : pageSize;

			var projectPredicate = _FilteringService.Filter<TaskTypeModel>().Where(m => m.ProjectCode == project);
			var predicate = filter.Concat(new[] {projectPredicate}).ToArray();

			return _FilteringDao.List<TaskTypeModel>(predicate, 
				new FilteringOptions { Skip = page*pageSize, Take = pageSize, Sorters = sorters})
				.Select(m => new TaskTypeDTO
				{
					Id = m.Id,
					Name = m.Name,
					Author = (m.Creator != null ? m.Creator.ShortName : string.Empty),
					CreationTime = m.CreationTime,
					ModelVersion = m.ModelVersion
				});
		}

		[JsonEndpoint, RequireAuthorization]
		public ValidationResult EditTaskType([NotEmpty] string project, [NotNull] TaskTypeDTO model)
		{
			var validationResult = new ValidationResult();

			try
			{
				var persistentModel = default(Guid).Equals(model.Id)
					? new TaskTypeModel { ProjectCode = project }//TODO improve AuthController for work without http context { Creator = authController.CurrentUser() }
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

		[JsonEndpoint, RequireAuthorization]
		public bool DeleteTaskType([NotEmpty] Guid id)
		{
			var taskType = _CrudDao.Get<TaskTypeModel>(id, true);

			if (_SessionProvider.CurrentSession.QueryOver<TaskModel>()
					.Where(m => m.TaskType == taskType).RowCount() > 0)
				throw new CannotDeleteReferencedItemException();

			_CrudDao.Delete(taskType);

			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> TaskTypeMetadata()
		{
			return MetadataForModelAndRelations<TaskTypeModel>();
		}
    }
}
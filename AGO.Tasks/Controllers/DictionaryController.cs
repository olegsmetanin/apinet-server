﻿using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Filters;
using AGO.Core.Filters.Metadata;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Processing;
using AGO.Core.Modules.Attributes;
using AGO.Tasks.Controllers.DTO;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;


namespace AGO.Tasks.Controllers
{
	/// <summary>
    /// Контроллер справочников модуля задач
    /// </summary>
    public class DictionaryController: AbstractTasksController
    {
        public DictionaryController(
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

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupTaskTypes(
			[NotEmpty] string project,
			string term,
			[InRange(0, null)] int page)
		{
			return Lookup<TaskTypeModel>(project, term, page, m => m.Name);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<TaskTypeDTO> GetTaskTypes(
			[NotEmpty] string project,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters, 
			[InRange(0, null)] int page)
		{
			var projectPredicate = _FilteringService.Filter<TaskTypeModel>().Where(m => m.ProjectCode == project);
			var predicate = filter.Concat(new[] {projectPredicate}).ToArray();
			var adapter = new TaskTypeAdapter();

			return _FilteringDao.List<TaskTypeModel>(predicate, page, sorters)
				.Select(adapter.Fill)
				.ToArray();
		}

		[JsonEndpoint, RequireAuthorization]
		public ValidationResult EditTaskType([NotEmpty] string project, [NotNull] TaskTypeDTO model)
		{
			return Edit<TaskTypeModel>(model.Id, project, (taskType, vr) => { taskType.Name = model.Name.TrimSafe(); });
		}

    	private void InternalDeleteTaskType(Guid id)
    	{
    		var taskType = _CrudDao.Get<TaskTypeModel>(id, true);

    		if (_CrudDao.Exists<TaskModel>(q => q.Where(m => m.TaskType == taskType)))
    			throw new CannotDeleteReferencedItemException();

    		_CrudDao.Delete(taskType);
    	}

		[JsonEndpoint, RequireAuthorization]
		public bool DeleteTaskType([NotEmpty] Guid id)
		{
			InternalDeleteTaskType(id);

			return true;
		}

		//TODO transaction management???
    	[JsonEndpoint, RequireAuthorization]
		public bool DeleteTaskTypes([NotEmpty] string project, [NotNull] ICollection<Guid> ids, Guid? replacementTypeId)
    	{
			if (replacementTypeId.HasValue && ids.Contains(replacementTypeId.Value))
				throw new InvalidOperationException(string.Format("Can't replace with task type, that will be deleted too"));

    		var s = _SessionProvider.CurrentSession;
    		var trn = s.BeginTransaction();
    		try
    		{
    			const string hqlUpdate =
    				"update versioned TaskModel set TaskTypeId = :newTypeId where ProjectCode = :project and TaskTypeId = :oldTypeId";
    			var updateQuery = s.CreateQuery(hqlUpdate);

    			foreach (var id in ids)
    			{
    				if (replacementTypeId.HasValue)
    				{
    					updateQuery
    						.SetGuid("newTypeId", replacementTypeId.Value)
    						.SetString("project", project)
    						.SetGuid("oldTypeId", id)
    						.ExecuteUpdate();
    				}

					InternalDeleteTaskType(id);
    			}

    			trn.Commit();
    		}
    		catch (Exception)
    		{
				trn.Rollback();
    			throw;
    		}
			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> TaskTypeMetadata()
		{
			return MetadataForModelAndRelations<TaskTypeModel>();
		}

		[JsonEndpoint, RequireAuthorization]
    	public IEnumerable<LookupEntry> LookupCustomStatuses(
			[NotEmpty] string project, 
			string term, 
			[InRange(0, null)] int page)
		{
			return Lookup<CustomTaskStatusModel>(project, term, page, m => m.Name);
    	}

		[JsonEndpoint, RequireAuthorization]
    	public IEnumerable<CustomStatusDTO> GetCustomStatuses(
			[NotEmpty] string project, 
			[NotNull] ICollection<IModelFilterNode> filter, 
			[NotNull] ICollection<SortInfo> sorters, 
			[InRange(0, null)] int page)
    	{
			var projectPredicate = _FilteringService.Filter<CustomTaskStatusModel>().Where(m => m.ProjectCode == project);
			var predicate = filter.Concat(new[] { projectPredicate }).ToArray();
			var adapter = new CustomStatusAdapter();

			return _FilteringDao
				.List<CustomTaskStatusModel>(predicate, page, sorters)
				.Select(adapter.Fill)
				.ToArray();
    	}

		[JsonEndpoint, RequireAuthorization]
		public ValidationResult EditCustomStatus([NotEmpty] string project, [NotNull] CustomStatusDTO model)
		{
			return Edit<CustomTaskStatusModel>(model.Id, project, 
				(status, vr) => 
					{
						status.Name = model.Name.TrimSafe();
						status.ViewOrder = model.ViewOrder;
					});
		}

		private void InternalDeleteCustomStatus(Guid id)
		{
			var status = _CrudDao.Get<CustomTaskStatusModel>(id, true);

			if (_CrudDao.Exists<TaskModel>(q => q.Where(m => m.CustomStatus == status)))
				throw new CannotDeleteReferencedItemException();

			if (_CrudDao.Exists<CustomTaskStatusHistoryModel>(q => q.Where(m => m.Status == status)))
				throw new CannotDeleteReferencedItemException();

			_CrudDao.Delete(status);
		}

		[JsonEndpoint, RequireAuthorization]
		public bool DeleteCustomStatus([NotEmpty] Guid id)
		{
			InternalDeleteCustomStatus(id);

			return true;
		}

		//TODO transaction management???
		[JsonEndpoint, RequireAuthorization]
		public bool DeleteCustomStatuses([NotEmpty] string project, [NotNull] ICollection<Guid> ids, Guid? replacementStatusId)
		{
			if (replacementStatusId.HasValue && ids.Contains(replacementStatusId.Value))
				throw new InvalidOperationException(string.Format("Can't replace with custom status, that will be deleted too"));

			var trn = Session.BeginTransaction();
			try
			{
				const string hqlTaskUpdate =
					"update versioned TaskModel set CustomStatusId = :newStatusId where ProjectCode = :project and CustomStatusId = :oldStatusId";
				const string hqlHistoryUpdate =
					"update versioned CustomTaskStatusHistoryModel set StatusId = :newStatusId where StatusId = :oldStatusId";
				var taskUpdateQuery = Session.CreateQuery(hqlTaskUpdate);
				var historyUpdateQuery = Session.CreateQuery(hqlHistoryUpdate);

				foreach (var id in ids)
				{
					if (replacementStatusId.HasValue)
					{
						taskUpdateQuery
							.SetGuid("newStatusId", replacementStatusId.Value)
							.SetString("project", project)
							.SetGuid("oldStatusId", id)
							.ExecuteUpdate();
						historyUpdateQuery
							.SetGuid("newStatusId", replacementStatusId.Value)
							.SetGuid("oldStatusId", id)
							.ExecuteUpdate();
					}

					InternalDeleteCustomStatus(id);
				}

				trn.Commit();
			}
			catch (Exception)
			{
				trn.Rollback();
				throw;
			}
			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> CustomTaskStatusMetadata()
		{
			return MetadataForModelAndRelations<CustomTaskStatusModel>();
		}
    }
}
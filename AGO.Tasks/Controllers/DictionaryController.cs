using System;
using System.Collections.Generic;
using System.Globalization;
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

		private static IDictionary<string, LookupEntry[]> taskStatuses;
		private static IDictionary<string, LookupEntry[]> taskPriorities;

		private IEnumerable<LookupEntry> LookupEnum<TEnum>(
			string term, 
			int page,
			ref IDictionary<string, LookupEntry[]> cache)
		{
			if (page > 0) return Enumerable.Empty<LookupEntry>(); //while size of enum less than defaul page size (10)

			var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
			if (cache == null)
			{
				//no need to locking - replace with same value from another thread has no negative effect
				cache = new Dictionary<string, LookupEntry[]>();
			}
			if (!cache.ContainsKey(lang))
			{
				//no need to locking - replace with same value from another thread has no negative effect
				cache[lang] = Enum.GetValues(typeof(TEnum))
					.OfType<TEnum>() //GetValues preserve enum order, no OrderBy used
					.Select(s => new LookupEntry 
								{
									Id = s.ToString(),
									Text = (_LocalizationService.MessageForType(s.GetType(), s) ?? s.ToString())
								})
					.ToArray();
			}

			if (term.IsNullOrWhiteSpace())
				return cache[lang];

			return cache[lang]
				.Where(l => l.Text.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0)
				.ToArray();
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupTaskStatuses(string term, [InRange(0, null)] int page)
		{
			return LookupEnum<TaskStatus>(term, page, ref taskStatuses);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupTaskPriorities(string term, [InRange(0, null)] int page)
		{
			return LookupEnum<TaskPriority>(term, page, ref taskPriorities);
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
		public int GetTaskTypesCount([NotEmpty] string project, [NotNull] ICollection<IModelFilterNode> filter)
		{
			var projectPredicate = _FilteringService.Filter<TaskTypeModel>().Where(m => m.ProjectCode == project);
			var predicate = filter.Concat(new[] { projectPredicate }).ToArray();

			return _FilteringDao.RowCount<TaskTypeModel>(predicate);
		}

		[JsonEndpoint, RequireAuthorization]
		public UpdateResult<TaskTypeDTO> EditTaskType([NotEmpty] string project, [NotNull] TaskTypeDTO model)
		{
			return Edit<TaskTypeModel, TaskTypeDTO>(model.Id, project, 
				(taskType, vr) => { taskType.Name = model.Name.TrimSafe(); },
				taskType => new TaskTypeAdapter().Fill(taskType));
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
				throw new CanNotReplaceWithItemThatWillBeDeletedTo();

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
    }
}
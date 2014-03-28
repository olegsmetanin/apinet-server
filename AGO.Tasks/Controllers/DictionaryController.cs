using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Controllers.Security;
using AGO.Core.DataAccess;
using AGO.Core.Filters;
using AGO.Core.Filters.Metadata;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Processing;
using AGO.Core.Modules.Attributes;
using AGO.Core.Security;
using AGO.Tasks.Controllers.DTO;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using NHibernate.Criterion;


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
			ILocalizationService localizationService,
			IModelProcessingService modelProcessingService,
			AuthController authController,
			ISecurityService securityService,
			ISessionProviderRegistry registry,
			DaoFactory factory)
			: base(jsonService, filteringService, localizationService, modelProcessingService, authController, securityService, registry, factory)
		{
		}

		private static IDictionary<string, LookupEntry[]> taskStatuses;
		private static IDictionary<string, LookupEntry[]> taskPriorities;

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
			var predicate = ApplyReadConstraint<TaskTypeModel>(project, filter.Concat(new[] {projectPredicate}).ToArray());
			var adapter = new TaskTypeAdapter();

			return DaoFactory.CreateProjectFilteringDao(project)
				.List<TaskTypeModel>(predicate, page, sorters)
				.Select(adapter.Fill)
				.ToArray();
		}

		[JsonEndpoint, RequireAuthorization]
		public int GetTaskTypesCount([NotEmpty] string project, [NotNull] ICollection<IModelFilterNode> filter)
		{
			var projectPredicate = _FilteringService.Filter<TaskTypeModel>().Where(m => m.ProjectCode == project);
			var predicate = ApplyReadConstraint<TaskTypeModel>(project, filter.Concat(new[] { projectPredicate }).ToArray());

			return DaoFactory.CreateProjectFilteringDao(project).RowCount<TaskTypeModel>(predicate);
		}

		[JsonEndpoint, RequireAuthorization]
		public UpdateResult<TaskTypeDTO> EditTaskType([NotEmpty] string project, [NotNull] TaskTypeDTO model)
		{
			return Edit<TaskTypeModel, TaskTypeDTO>(model.Id, project, 
				(taskType, vr) => { taskType.Name = model.Name.TrimSafe(); },
				taskType => new TaskTypeAdapter().Fill(taskType));
		}

    	private void InternalDeleteTaskType(string project, Guid id, ICrudDao dao = null)
    	{
    		dao = dao ?? DaoFactory.CreateProjectCrudDao(project);
    		var taskType = SecureFind<TaskTypeModel>(project, id);

			DemandDelete(taskType, taskType.ProjectCode);

    		if (dao.Exists<TaskModel>(q => q.Where(m => m.TaskType.Id == taskType.Id)))
    			throw new CannotDeleteReferencedItemException();

    		dao.Delete(taskType);
    	}

		[JsonEndpoint, RequireAuthorization]
		public bool DeleteTaskType([NotEmpty] string project, [NotEmpty] Guid id)
		{
			InternalDeleteTaskType(project, id);

			return true;
		}

    	[JsonEndpoint, RequireAuthorization]
		public bool DeleteTaskTypes([NotEmpty] string project, [NotNull] ICollection<Guid> ids, Guid? replacementTypeId)
    	{
			if (replacementTypeId.HasValue && ids.Contains(replacementTypeId.Value))
				throw new CanNotReplaceWithItemThatWillBeDeletedTo();

    		var s = ProjectSession(project);
    		var trn = s.BeginTransaction();
    		try
    		{
    			const string hqlUpdate =
    				"update versioned TaskModel set TaskTypeId = :newTypeId where ProjectCode = :project and TaskTypeId = :oldTypeId";
    			var updateQuery = s.CreateQuery(hqlUpdate);
    			var dao = DaoFactory.CreateProjectCrudDao(project);
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

					InternalDeleteTaskType(project, id, dao);
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
		public IEnumerable<IModelMetadata> TaskTypeMetadata([NotEmpty] string project)
		{
			return MetadataForModelAndRelations<TaskTypeModel>(project);
		}

		[Obsolete("Use core controller for all tags activity")]
		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupTags(
			[NotEmpty] string project, 
			string term,
			[InRange(0, null)] int page)
		{
			return Lookup<TaskTagModel>(project, term, page, m => m.FullName);
		}
    }
}

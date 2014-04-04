using System;
using System.Collections.Generic;
using AGO.Core;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Controllers.Activity;
using AGO.Core.Controllers.Security;
using AGO.Core.DataAccess;
using AGO.Core.Filters;
using AGO.Core.Filters.Metadata;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Processing;
using AGO.Core.Modules.Attributes;
using AGO.Core.Security;
using AGO.Tasks.Controllers.Activity;
using AGO.Tasks.Model.Task;


namespace AGO.Tasks.Controllers
{
    public class ActivityController: AbstractActivityController
	{
		#region Properties, fields, constructors

		public ActivityController(
			IJsonService jsonService,
			IFilteringService filteringService,
			ILocalizationService localizationService,
			IModelProcessingService modelProcessingService,
			AuthController authController,
			ISecurityService securityService,
			ISessionProviderRegistry registry,
			DaoFactory factory,
			ProjectAttributesActivityViewProcessor projectAttributesProcessor,
			TaskAttributesActivityViewProcessor taskAttributesProcessor,
			TaskCustomPropertiesActivityViewProcessor taskCustomPropertiesProcessor,
			IEnumerable<IActivityViewProcessor> activityViewProcessors,
			ProjectTasksRelatedActivityViewProcessor projectTasksViewProcessor,
			TaskAgreementsRelatedActivityViewProcessor taskAgreementsViewProcessor,
			TaskExecutorsRelatedActivityViewProcessor taskExecutorsViewProcessor,
			TaskFilesRelatedActivityViewProcessor taskFilesViewProcessor,
            TaskCommentsRelatedActivityViewProcessor taskCommentsViewProcessor)
			: base(jsonService, filteringService, localizationService, modelProcessingService, authController, securityService, registry, factory, activityViewProcessors)
		{
			_ActivityViewProcessors.Add(projectAttributesProcessor);
			_ActivityViewProcessors.Add(taskAttributesProcessor);
			_ActivityViewProcessors.Add(taskCustomPropertiesProcessor);		
			_ActivityViewProcessors.Add(projectTasksViewProcessor);
			_ActivityViewProcessors.Add(taskAgreementsViewProcessor);
			_ActivityViewProcessors.Add(taskExecutorsViewProcessor);
			_ActivityViewProcessors.Add(taskFilesViewProcessor);
            _ActivityViewProcessors.Add(taskCommentsViewProcessor);
		} 

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<ActivityView> GetActivities(
			[NotEmpty] string project,
			[NotNull] ICollection<IModelFilterNode> filter,
			string taskNum,			
			ActivityPredefinedFilter predefined,
			DateTime specificDate)
		{
			var session = ProjectSession(project);

			var itemId = default(Guid);
			if(!taskNum.IsNullOrWhiteSpace())
					itemId = session.QueryOver<TaskModel>().Where(m => m.SeqNumber == taskNum && m.ProjectCode == project)
				.Select(m => m.Id).Take(1).SingleOrDefault<Guid>();

			var criteria = MakeActivityCriteria(project, filter, itemId, predefined, specificDate);
			var dao = DaoFactory.CreateProjectCrudDao(project);

			return ActivityViewsFromRecords(dao.Future<ActivityRecordModel>(criteria, new FilteringOptions 
			{ 
				Page = 0,
				PageSize = 0,
				Sorters = new[] { new SortInfo { Property = "CreationTime", Descending = true} }
			}), default(Guid).Equals(itemId));
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> ActivityMetadata()
		{
			return MetadataForModelAndRelations<ActivityRecordModel>();
		}

		#endregion
    }
}
using System;
using System.Collections.Generic;
using AGO.Core;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Controllers.Activity;
using AGO.Core.Controllers.Security;
using AGO.Core.Filters;
using AGO.Core.Filters.Metadata;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Processing;
using AGO.Core.Modules.Attributes;
using AGO.Core.Security;
using AGO.Tasks.Controllers.Activity;


namespace AGO.Tasks.Controllers
{
    public class ActivityController: AbstractActivityController
	{
		#region Properties, fields, constructors

		public ActivityController(
			IJsonService jsonService,
			IFilteringService filteringService,
			ICrudDao crudDao,
			IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			ILocalizationService localizationService,
			IModelProcessingService modelProcessingService,
			AuthController authController,
			ISecurityService securityService,
			IEnumerable<IActivityViewProcessor> activityViewProcessors,
			TaskCollectionActivityViewProcessor taskCollectionProcessor,
			TaskAttributeActivityViewProcessor taskAttributeProcessor)
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, localizationService, modelProcessingService, authController, securityService, activityViewProcessors)
		{
			_ActivityViewProcessors.Add(taskCollectionProcessor);
			_ActivityViewProcessors.Add(taskAttributeProcessor);
		} 

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<ActivityView> GetActivities(
			[NotEmpty] string project,
			[NotNull] ICollection<IModelFilterNode> filter,
			Guid itemId,
			ActivityPredefinedFilter predefined,
			DateTime specificDate)
		{
			var criteria = MakeActivityCriteria(project, filter, itemId, predefined, specificDate);

			return ActivityViewsFromRecords(_CrudDao.Future<ActivityRecordModel>(criteria, new FilteringOptions 
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
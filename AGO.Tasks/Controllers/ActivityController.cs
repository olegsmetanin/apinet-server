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
			IEnumerable<IActivityViewProcessor> activityViewProcessors,
			TaskCollectionActivityViewProcessor taskCollectionProcessor,
			TaskAttributeActivityViewProcessor taskAttributeProcessor)
			: base(jsonService, filteringService, localizationService, modelProcessingService, authController, securityService, registry, factory, activityViewProcessors)
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
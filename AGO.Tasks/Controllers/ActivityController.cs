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
using AGO.Core.Model.Projects;
using AGO.Core.Modules.Attributes;
using AGO.Core.Security;
using AGO.Tasks.Controllers.Activity;
using AGO.Tasks.Model.Task;
using NHibernate.Criterion;


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
			var projectModel = _CrudDao.Find<ProjectModel>(q => q.Where(m => m.ProjectCode == project));
			if (projectModel == null)
				throw new NoSuchProjectException();

			var user = _AuthController.CurrentUser();
			var member = _CrudDao.Find<ProjectMemberModel>(q => q.Where(m => m.ProjectCode == projectModel.ProjectCode && m.UserId == user.Id));
			if (member == null)
				throw new NoSuchProjectMemberException();

			var criteria = MakeActivityCriteria(project, filter, itemId, predefined, specificDate);
			if (TaskProjectRoles.Executor.Equals(member.CurrentRole))
			{
				criteria.Add(Subqueries.PropertyIn("ItemId", DetachedCriteria.For<TaskExecutorModel>()
					.Add(Restrictions.Eq("ExecutorId", member.Id))
					.SetProjection(Projections.Property("TaskId"))));
			}

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
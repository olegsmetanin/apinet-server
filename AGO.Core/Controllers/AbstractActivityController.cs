using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Controllers.Activity;
using AGO.Core.Controllers.Security;
using AGO.Core.Localization;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Processing;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Model.Security;
using AGO.Core.Security;
using NHibernate;


namespace AGO.Core.Controllers
{
	public class AbstractActivityController : AbstractController
	{
		#region Properties, fields, constructors

		protected readonly IList<IActivityViewProcessor> _ActivityViewProcessors;

		public AbstractActivityController(
			IJsonService jsonService,
			IFilteringService filteringService,
			ICrudDao crudDao,
			IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			ILocalizationService localizationService,
			IModelProcessingService modelProcessingService,
			AuthController authController,
			ISecurityService securityService,
			IEnumerable<IActivityViewProcessor> activityViewProcessors)
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, localizationService, modelProcessingService, authController, securityService)
		{
			if (activityViewProcessors == null)
				throw new ArgumentNullException("activityViewProcessors");
			_ActivityViewProcessors = new List<IActivityViewProcessor>(activityViewProcessors);
		}

		#endregion

		#region Template methods
		
		protected virtual UserModel CurrentUser
		{
			get { return _AuthController.CurrentUser(); }
		}

		protected virtual ActivityView ActivityViewFromRecord(ActivityRecordModel record)
		{
			return new ActivityView(record.ItemId);
		}

		protected virtual ActivityItemView ActivityItemViewFromRecord(ActivityRecordModel record)
		{
			return new ActivityItemView(record.ItemId);
		}

		protected virtual bool IsNewActivityViewRequired(ActivityView currentView, ActivityRecordModel record, ActivityRecordModel prevRecord)
		{
			if (currentView == null || prevRecord == null)
				return true;
			
			if (!currentView.ItemId.Equals(record.ItemId))
				return true;

			var prevDate = (prevRecord.CreationTime ?? DateTime.Now).ToLocalTime();
			prevDate = new DateTime(prevDate.Year, prevDate.Month, prevDate.Day);
			
			var date = (record.CreationTime ?? DateTime.Now).ToLocalTime();
			date = new DateTime(date.Year, date.Month, date.Day);

			return !date.Equals(prevDate);
		}
		
		#endregion

		#region Helper methods
		
		protected ICriteria MakeActivityCriteria(string project, IEnumerable<IModelFilterNode> filter, ActivityPredefinedFilter predefined)
		{
			return _FilteringService.CompileFilter(SecurityService.ApplyReadConstraint<ActivityRecordModel>(project, CurrentUser.Id, Session,
				filter.Concat(new[] { predefined.ToFilter(_FilteringService.Filter<ActivityRecordModel>()) }).ToArray()), typeof(ActivityRecordModel))
				.GetExecutableCriteria(Session);
		}

		protected IList<ActivityView> ActivityViewsFromRecords(IEnumerable<ActivityRecordModel> records)
		{
			var result = new List<ActivityView>();

			Action<ActivityView> postProcess = view =>
			{
				if (view == null)
					return;
					
				view.Items = view.Items.Reverse().ToList();

				var currentUser = string.Empty;
				foreach (var itemView in view.Items)
				{
					if (string.Equals(itemView.User, currentUser))
						itemView.User = string.Empty;
					else
						currentUser = itemView.User;
				}
			};

			ActivityView currentView = null;
			ActivityRecordModel prevRecord = null;

			foreach (var record in records)
			{
				if (IsNewActivityViewRequired(currentView, record, prevRecord))
				{
					postProcess(currentView);

					currentView = ActivityViewFromRecord(record);
					if (currentView != null)
					{
						foreach (var processor in _ActivityViewProcessors)
							processor.Process(currentView, record);
						result.Add(currentView);
					}
				}

				prevRecord = record;

				if (currentView == null)
					continue;

				var itemView = ActivityItemViewFromRecord(record);
				if (itemView == null)
					continue;

				foreach (var processor in _ActivityViewProcessors)
					processor.ProcessItem(itemView, record);
				currentView.Items.Add(itemView);
			}

			postProcess(currentView);

			return result;
		}

		#endregion
	}
}
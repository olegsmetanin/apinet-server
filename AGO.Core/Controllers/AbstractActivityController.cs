﻿using System;
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

		protected virtual ActivityView ActivityViewFromRecord(ActivityRecordModel record, bool grouping)
		{
			return new ActivityView(record.ItemId, record.ItemType, grouping);
		}

		protected virtual ActivityItemView ActivityItemViewFromRecord(ActivityRecordModel record, bool grouping)
		{
			return !grouping
				? new ActivityItemView(record.ItemId, record.ItemType, record.GetType()) 
				: new GroupedActivityItemView(record.ItemId, record.ItemType, record.GetType());
		}

		protected virtual bool IsNewActivityViewRequired(
			ActivityView currentView, 
			ActivityRecordModel record, 
			ActivityRecordModel prevRecord)
		{
			if (currentView == null || prevRecord == null || !currentView.ItemId.Equals(record.ItemId))
				return true;
			
			var prevDate = (prevRecord.CreationTime ?? DateTime.Now).ToLocalTime();
			prevDate = new DateTime(prevDate.Year, prevDate.Month, prevDate.Day);
			
			var date = (record.CreationTime ?? DateTime.Now).ToLocalTime();
			date = new DateTime(date.Year, date.Month, date.Day);

			return !date.Equals(prevDate);
		}

		protected virtual ActivityItemView GetExistingActivityItemView(
			ActivityView currentView, 
			ActivityRecordModel record,
			bool grouping)
		{
			if (!grouping || currentView == null)
				return null;

			foreach (var groupedView in currentView.Items.OfType<GroupedActivityItemView>())
			{
				if (!groupedView.ItemId.Equals(record.ItemId))
					continue;			
				if (record is AttributeChangeActivityRecordModel && ChangeType.Update.ToString().Equals(groupedView.Action))
					return groupedView;

				var collectionChangeRecord = record as CollectionChangeActivityRecordModel;
				if (collectionChangeRecord != null && collectionChangeRecord.ChangeType.ToString().Equals(groupedView.Action) &&
						collectionChangeRecord.RelatedItemType.Equals(groupedView.Before))
					return groupedView;
			}

			return null;
		}
		
		#endregion

		#region Helper methods
		
		protected ICriteria MakeActivityCriteria(
			string project, 
			IEnumerable<IModelFilterNode> filter,
			Guid itemId,
			ActivityPredefinedFilter predefined,
			DateTime specificDate)
		{
			return _FilteringService.CompileFilter(SecurityService.ApplyReadConstraint<ActivityRecordModel>(project, CurrentUser.Id, Session,
				filter.Concat(new[]
				{
					predefined.ToFilter(specificDate, _FilteringService.Filter<ActivityRecordModel>()),
					!default(Guid).Equals(itemId) ? _FilteringService.Filter<ActivityRecordModel>().Where(m => m.ItemId == itemId) : null
				}).ToArray()), typeof(ActivityRecordModel))
				.GetExecutableCriteria(Session);
		}

		protected IList<ActivityView> ActivityViewsFromRecords(IEnumerable<ActivityRecordModel> records, bool grouping)
		{
			var result = new List<ActivityView>();

			Action<ActivityView> postProcess = view =>
			{
				if (view == null)
					return;

				foreach (var processor in _ActivityViewProcessors)
					processor.PostProcess(view);
				
				foreach (var item in view.Items)
				{
					foreach (var processor in _ActivityViewProcessors)
						processor.PostProcessItem(item);
				}

				view.Items = view.Items.Reverse().ToList();
			};

			ActivityView currentView = null;
			ActivityRecordModel prevRecord = null;

			foreach (var record in records)
			{
				if (IsNewActivityViewRequired(currentView, record, prevRecord))
				{
					postProcess(currentView);

					currentView = ActivityViewFromRecord(record, grouping);
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

				var existingItem = GetExistingActivityItemView(currentView, record, grouping);
				var currentItemView = existingItem ?? ActivityItemViewFromRecord(record, grouping);
				if (existingItem == null)
					currentView.Items.Add(currentItemView);

				foreach (var processor in _ActivityViewProcessors)
					processor.ProcessItem(currentItemView, record);
			}

			postProcess(currentView);

			return result;
		}

		#endregion
	}
}
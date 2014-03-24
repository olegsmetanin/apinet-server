using System;
using System.Globalization;
using System.Linq;
using AGO.Core.Localization;
using AGO.Core.Model.Activity;

namespace AGO.Core.Controllers.Activity
{
	public abstract class AbstractActivityViewProcessor<TModel> : IActivityViewProcessor
		where TModel : ActivityRecordModel
	{
		#region Properties, fields, constructors

		protected readonly ICrudDao _CrudDao;

		protected readonly ISessionProvider _SessionProvider;

		protected readonly ILocalizationService _LocalizationService;

		protected AbstractActivityViewProcessor(
			ICrudDao crudDao,
			ISessionProvider sessionProvider,
			ILocalizationService localizationService)
		{
			if (crudDao == null)
				throw new ArgumentNullException("crudDao");
			_CrudDao = crudDao;

			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");
			_SessionProvider = sessionProvider;

			if (localizationService == null)
				throw new ArgumentNullException("localizationService");
			_LocalizationService = localizationService;
		}

		#endregion

		#region Interfaces implementation

		public void Process(ActivityView view, ActivityRecordModel model)
		{
			if (view == null || !(model is TModel))
				return;
			DoProcess(view, (TModel) model);
		}

		public void PostProcess(ActivityView view)
		{
			if (view == null)
				return;
			DoPostProcess(view);
		}

		public void ProcessItem(ActivityItemView view, ActivityRecordModel model)
		{
			if (view == null || !(model is TModel))
				return;
			DoProcessItem(view, (TModel) model);
		}

		public void PostProcessItem(ActivityItemView view)
		{
			if (view == null)
				return;
			DoPostProcessItem(view);
		}

		#endregion

		#region Template methods

		protected virtual void DoProcess(ActivityView view, TModel model)
		{
			view.ActivityTime = (model.CreationTime ?? DateTime.Now).ToLocalTime().ToString("O");
			view.ActivityItem = model.ItemName;
		}

		protected virtual void DoProcessItem(ActivityItemView view, TModel model)
		{
			var groupedView = view as GroupedActivityItemView;
			if (groupedView != null)
			{
				groupedView.ChangeCount++;
				groupedView.Users.Add(model.Creator.ToStringSafe()); 
				return;
			}

			view.ActivityTime = (model.CreationTime ?? DateTime.Now).ToLocalTime().ToString("O");
			view.User = model.Creator.ToStringSafe();
		}

		protected virtual void DoPostProcess(ActivityView view)
		{
		}

		protected virtual void DoPostProcessItem(ActivityItemView view)
		{
			LocalizeUser(view);
		}

		#endregion
		
		#region Helper methods
		
		protected virtual void LocalizeActivityItem<TType>(ActivityView view)
		{
			if (view.ActivityItem.IsNullOrWhiteSpace())
				return;

			view.ActivityItem = _LocalizationService.MessageForType(typeof(TType), "ActivityItem",
				CultureInfo.CurrentUICulture, view.ActivityItem);
		}
		
		protected virtual void LocalizeUser(ActivityItemView view)
		{
			var groupedView = view as GroupedActivityItemView;
			if (groupedView == null)
			{
				if (!view.User.IsNullOrWhiteSpace())
						view.User = _LocalizationService.MessageForType(typeof(IActivityViewProcessor), "User",
					CultureInfo.CurrentUICulture, view.User);
				return;
			}				

			groupedView.User = groupedView.Users.Count > 1 
				? _LocalizationService.MessageForType(typeof(IActivityViewProcessor), "Users", CultureInfo.CurrentUICulture,
					groupedView.Users.Aggregate(string.Empty, (current, user) => current.IsNullOrWhiteSpace() ? user : current + ", " + user))
				: _LocalizationService.MessageForType(typeof(IActivityViewProcessor), "User", CultureInfo.CurrentUICulture, 
					groupedView.Users.FirstOrDefault());
		}

		#endregion
	}
}
using System;
using System.Globalization;
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

		public void ProcessItem(ActivityItemView view, ActivityRecordModel model)
		{
			if (view == null || !(model is TModel))
				return;
			DoProcessItem(view, (TModel) model);
		}

		#endregion

		#region Template methods

		protected virtual void DoProcess(ActivityView view, TModel model)
		{
			view.ActivityTime = (model.CreationTime ?? DateTime.Now).ToLocalTime().ToString("D", CultureInfo.CurrentUICulture);
			view.ActivityItem = model.ItemName;
		}
	
		protected abstract void DoProcessItem(ActivityItemView view, TModel model);

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
			if (view.User.IsNullOrWhiteSpace())
				return;

			view.User = _LocalizationService.MessageForType(typeof(IActivityViewProcessor), "User",
				CultureInfo.CurrentUICulture, view.User);
		}

		#endregion
	}
}
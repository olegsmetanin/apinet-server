using System.Globalization;
using AGO.Core.Localization;
using AGO.Core.Model.Activity;

namespace AGO.Core.Controllers.Activity
{
	public class RelatedChangeActivityViewProcessor : AbstractActivityViewProcessor<RelatedChangeActivityRecordModel>
	{
		#region Properties, fields, constructors
		public RelatedChangeActivityViewProcessor(ILocalizationService localizationService)
			: base(localizationService)
		{		
		}

		#endregion

		#region Template methods

		protected override void DoProcessItem(ActivityItemView view, RelatedChangeActivityRecordModel model)
		{
			base.DoProcessItem(view, model);

			view.Action = model.ChangeType.ToString();
			view.Before = model.RelatedItemName;
		}

		protected override void DoPostProcessItem(ActivityItemView view)
		{
			base.DoPostProcessItem(view);
			if (typeof(RelatedChangeActivityRecordModel) != view.RecordType)
				return;

			LocalizeAction(view);
		}


		protected virtual void LocalizeAction(ActivityItemView view)
		{
			if (view.Action.IsNullOrWhiteSpace())
				return;

			view.Action = LocalizationService.MessageForType(typeof(RelatedChangeActivityViewProcessor), 
				"Action" + view.Action, CultureInfo.CurrentUICulture);
		}

		protected virtual void LocalizeRelatedActivityItem<TType>(ActivityItemView view)
		{
			if (view.Before.IsNullOrWhiteSpace())
				return;
			view.Before = LocalizationService.MessageForType(typeof(TType), "RelatedActivityItem",
				CultureInfo.CurrentUICulture, view.Before);
		}

		#endregion
	}
}
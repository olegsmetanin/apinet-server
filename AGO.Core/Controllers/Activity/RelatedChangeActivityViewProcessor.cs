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

		protected override bool DoProcessItem(ActivityItemView view, RelatedChangeActivityRecordModel model)
		{
			view.Action = !(view is GroupedActivityItemView) ? model.ChangeType.ToString() : ChangeType.Update.ToString();
			view.Before = !(view is GroupedActivityItemView) ? model.RelatedItemName : string.Empty;

			return base.DoProcessItem(view, model);
		}
		
		protected override void LocalizeAction(ActivityItemView view)
		{
			base.LocalizeAction(view);

			if (view.Action.IsNullOrWhiteSpace() || view is GroupedActivityItemView)
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
using System;
using System.Globalization;
using AGO.Core.Localization;
using AGO.Core.Model;
using AGO.Core.Model.Activity;

namespace AGO.Core.Controllers.Activity
{
	public class AttributeChangeActivityViewProcessor : AbstractActivityViewProcessor<AttributeChangeActivityRecordModel>
	{
		#region Properties, fields, constructors
		public AttributeChangeActivityViewProcessor(ILocalizationService localizationService)
			: base(localizationService)
		{		
		}

		#endregion

		#region Template methods

		protected override bool DoProcessItem(ActivityItemView view, AttributeChangeActivityRecordModel model)
		{
			var groupedView = view as GroupedActivityItemView;
			if (groupedView != null)
			{
				groupedView.Action = groupedView.Action.IsNullOrWhiteSpace() ? ChangeType.Update.ToString() : groupedView.Action;
				return base.DoProcessItem(view, model);
			}

			view.Action = model.Attribute;
			view.Before = model.OldValue;
			view.After = model.NewValue;
			view.AdditionalInfo = model.AdditionalInfo;

			return base.DoProcessItem(view, model);
		}

		protected override void DoPostProcessItem(ActivityItemView view)
		{
			base.DoPostProcessItem(view);

			LocalizeValues(view);
		}

		protected virtual string GetLocalizedAttributeName(Type type, string attribute)
		{
			var localized = LocalizationService.MessageForType(type, attribute);
			if (localized.IsNullOrWhiteSpace())
			{
				var currentType = type;
				while (currentType != null && typeof(ICoreModel).IsAssignableFrom(currentType))
				{
					localized = LocalizationService.MessageForType(typeof(ICoreModel),
						string.Format("{0}.{1}", currentType.Name.RemoveSuffix("`1"), attribute));
					if (!localized.IsNullOrWhiteSpace())
						break;
					currentType = currentType.BaseType;
				}
			}

			return localized.IsNullOrWhiteSpace() ? attribute : localized;
		}

		protected virtual void LocalizeAttribute<TModel>(ActivityItemView view)
		{
			base.LocalizeAction(view);
			if (view.Action.IsNullOrWhiteSpace() || view is GroupedActivityItemView)
				return;

			view.Action = LocalizationService.MessageForType(typeof(AttributeChangeActivityViewProcessor), "Action", 
				CultureInfo.CurrentUICulture, GetLocalizedAttributeName(typeof(TModel), view.Action));
		}

		protected virtual void LocalizeValues(ActivityItemView view)
		{
			if (view is GroupedActivityItemView)
				return;

			Func<string, string, string> process = (str, key) =>
			{
				if (str.IsNullOrWhiteSpace())
					return str;

				return str.IsNullOrWhiteSpace() ? str : LocalizationService.MessageForType(
					typeof(AttributeChangeActivityViewProcessor), key, CultureInfo.CurrentUICulture, str);
			};

			view.Before = process(view.Before, "Before");
			view.After = process(view.After, "After");
		}

		protected virtual void LocalizeValuesByType<T>(ActivityItemView view)
		{
			if (view is GroupedActivityItemView)
				return;

			view.Before = LocalizationService.MessageForType(typeof(T), view.Before) ?? view.Before;
			view.After = LocalizationService.MessageForType(typeof(T), view.After) ?? view.After;
		}
		
		protected virtual void TransformDateValues(ActivityItemView view, bool includeTime = false)
		{
			if (view is GroupedActivityItemView)
				return;

			Func<string, string> process = str =>
			{
				DateTime date;
				if (!DateTime.TryParse(str, out date))
					return str;

				date = date.ToLocalTime();
				var result = date.ToString("d", CultureInfo.CurrentUICulture);
				if(includeTime)
					result += " " + date.ToString("t", CultureInfo.CurrentUICulture);

				return result;
			};

			view.Before = process(view.Before);
			view.After = process(view.After);
		}
		
		#endregion
	}
}
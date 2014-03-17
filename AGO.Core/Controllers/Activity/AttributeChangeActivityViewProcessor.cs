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
		public AttributeChangeActivityViewProcessor(
			ICrudDao crudDao,
			ISessionProvider sessionProvider,
			ILocalizationService localizationService)
			: base(crudDao, sessionProvider, localizationService)
		{		
		}

		#endregion

		#region Template methods

		protected override void DoProcessItem(ActivityItemView view, AttributeChangeActivityRecordModel model)
		{
			base.DoProcessItem(view, model);

			var groupedView = view as GroupedActivityItemView;
			if (groupedView != null)
			{
				groupedView.Action = groupedView.Action.IsNullOrWhiteSpace() ? ChangeType.Update.ToString() : groupedView.Action;
				return;
			}

			view.Action = model.Attribute;
			view.Before = model.OldValue;
			view.After = model.NewValue;			
		}

		protected override void DoPostProcessItem(ActivityItemView view)
		{
			base.DoPostProcessItem(view);
			if (typeof (AttributeChangeActivityRecordModel) != view.RecordType)
				return;

			LocalizeValues(view);
		}

		protected virtual string GetLocalizedAttributeName(Type type, string attribute)
		{
			var localized = _LocalizationService.MessageForType(type, attribute);
			if (localized.IsNullOrWhiteSpace())
			{
				var currentType = type;
				while (currentType != null && typeof(ICoreModel).IsAssignableFrom(currentType))
				{
					localized = _LocalizationService.MessageForType(typeof(ICoreModel),
						string.Format("{0}.{1}", currentType.Name.RemoveSuffix("`1"), attribute));
					if (!localized.IsNullOrWhiteSpace())
						break;
					currentType = currentType.BaseType;
				}
			}

			return localized.IsNullOrWhiteSpace() ? attribute : localized;
		}

		protected virtual void LocalizeAction<TModel>(ActivityItemView view)
		{
			if (view.Action.IsNullOrWhiteSpace() && ChangeType.Update.ToString().Equals(view.Action))
				return;

			if (view is GroupedActivityItemView)
			{
				view.Action = _LocalizationService.MessageForType(typeof(AttributeChangeActivityViewProcessor), "Updated",
					CultureInfo.CurrentUICulture, view.Action);
				return;
			}

			view.Action = _LocalizationService.MessageForType(typeof(AttributeChangeActivityViewProcessor), "Action", 
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

				return str.IsNullOrWhiteSpace() ? str : _LocalizationService.MessageForType(
					typeof(AttributeChangeActivityViewProcessor), key, CultureInfo.CurrentUICulture, str);
			};

			view.Before = process(view.Before, "Before");
			view.After = process(view.After, "After");
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
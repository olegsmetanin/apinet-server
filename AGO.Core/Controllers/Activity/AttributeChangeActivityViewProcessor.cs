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
			view.ActivityTime = (model.CreationTime ?? DateTime.Now).ToLocalTime().ToString("t", CultureInfo.CurrentUICulture);
			view.User = model.Creator.ToStringSafe();
			view.Action = model.Attribute;
			view.Before = model.OldValue;
			view.After = model.NewValue;

			LocalizeUser(view);
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
			if (view.Action.IsNullOrWhiteSpace())
				return;

			view.Action = _LocalizationService.MessageForType(typeof(AttributeChangeActivityViewProcessor), "Action", 
				CultureInfo.CurrentUICulture, GetLocalizedAttributeName(typeof(TModel), view.Action));
		}

		protected virtual void LocalizeValues(ActivityItemView view)
		{
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
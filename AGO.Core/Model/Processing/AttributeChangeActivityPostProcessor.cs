using System;
using System.Collections.Generic;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Security;

namespace AGO.Core.Model.Processing
{
	public abstract class AttributeChangeActivityPostProcessor<TModel> : ActivityPostProcessor<TModel>
		where TModel : SecureModel<Guid>, new()
	{
		#region Properties, fields, constructors

		protected AttributeChangeActivityPostProcessor(
			ICrudDao crudDao,
			ISessionProvider sessionProvider)
			: base(crudDao, sessionProvider)
		{
		}

		#endregion

		#region Template methods

		protected void CheckAttribute(
			IList<ActivityRecordModel> records,
			TModel model,
			TModel original,
			string attribute)
		{
			CheckAttributeInternal(records, model, attribute, original.GetMemberValue(attribute),
				model.GetMemberValue(attribute), original.IsNew());
		}

		protected void CheckAttribute(
			IList<ActivityRecordModel> records,
			TModel model,
			TModel original,
			string attribute,
			object oldValue,
			object newValue)
		{
			CheckAttributeInternal(records, model, attribute, oldValue, newValue, original.IsNew());
		}

		protected void CheckAttributeInternal(
			IList<ActivityRecordModel> records, 
			TModel model,
			string attribute,
			object oldValue, 
			object newValue,
			bool isNew = false)
		{			
			if (oldValue == null && newValue == null)
				return;

			if (!isNew && Equals(newValue, oldValue))
				return;

			var oldStrValue = (isNew ? null : oldValue.ConvertSafe<string>()) ?? string.Empty;
			var newStrValue = newValue.ConvertSafe<string>() ?? string.Empty;
			
			var oldDate = oldValue as DateTime?;
			if (!isNew && oldDate != null)
				oldStrValue = oldDate.Value.ToUniversalTime().ToString("O");

			var newDate = newValue as DateTime?;
			if (newDate != null)
				newStrValue = newDate.Value.ToUniversalTime().ToString("O");
			
			records.Add(PopulateActivityRecord(model, new AttributeChangeActivityRecordModel
			{
				Attribute = attribute.TrimSafe(),
				OldValue = oldStrValue,
				NewValue = newStrValue
			}));
		}

		#endregion
	}
}
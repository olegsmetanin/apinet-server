using System.Collections.Generic;
using AGO.Core.DataAccess;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Projects;

namespace AGO.Core.Model.Processing
{
	public abstract class CustomPropertyChangeActivityPostProcessor<TModel> : AttributeChangeActivityPostProcessor<TModel>
		where TModel : CustomPropertyInstanceModel, new()
	{
		#region Properties, fields, constructors

		protected CustomPropertyChangeActivityPostProcessor(
			DaoFactory factory,
			ISessionProviderRegistry providerRegistry)
			: base(factory, providerRegistry)
		{
		}

		#endregion

		#region Template methods

		protected override IList<ActivityRecordModel> RecordsForUpdate(TModel model, TModel original)
		{
			var result = new List<ActivityRecordModel>();

			CheckAttribute(result, model, original, "StringValue");
			CheckAttribute(result, model, original, "NumberValue");
			CheckAttribute(result, model, original, "DateValue");

			return result;
		}

		protected override AttributeChangeActivityRecordModel PopulateActivityRecord(TModel model, AttributeChangeActivityRecordModel record, ProjectMemberModel member = null)
		{
			record = base.PopulateActivityRecord(model, record, member);
			if (model.PropertyType == null)
				return record;

			record.Attribute = model.PropertyType.FullName ?? model.PropertyType.ToStringSafe();
			record.AdditionalInfo = model.PropertyType.ValueType.ToString();

			return record;
		}

		#endregion
	}
}
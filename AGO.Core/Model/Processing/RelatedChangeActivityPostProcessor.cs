using System;
using System.Collections.Generic;
using AGO.Core.DataAccess;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;

namespace AGO.Core.Model.Processing
{
	public abstract class RelatedChangeActivityPostProcessor<TModel, TRelatedModel> : ActivityPostProcessor<TModel, RelatedChangeActivityRecordModel>
		where TModel : IdentifiedModel<Guid>, new()
		where TRelatedModel : IdentifiedModel<Guid>
	{
		#region Properties, fields, constructors

		protected RelatedChangeActivityPostProcessor(
			DaoFactory factory,
			ISessionProviderRegistry providerRegistry)
			: base(factory, providerRegistry)
		{
		}

		#endregion

		#region Template methods

		protected virtual IList<ActivityRecordModel> RecordsForInsertion(TModel model)
		{
			return new List<ActivityRecordModel>();
		}

		protected override IList<ActivityRecordModel> RecordsForUpdate(TModel model, TModel original)
		{
			return !original.IsNew() ? new List<ActivityRecordModel>() : RecordsForInsertion(model);
		}

		protected virtual ActivityRecordModel PopulateRelatedActivityRecord(
			TModel model, 
			TRelatedModel relatedModel,
			RelatedChangeActivityRecordModel record, 
			ChangeType changeType,
			ProjectMemberModel member = null)
		{
			var relatedSecureModel = relatedModel as ISecureModel;
			member = member ?? (relatedSecureModel != null ? relatedSecureModel.LastChanger ?? relatedSecureModel.Creator: null);

			record.ItemType = relatedModel.GetType().Name;
			record.ItemName = relatedModel.ToStringSafe();
			record.ItemId = relatedModel.Id;

			record.Creator = member;
			record.RelatedItemType = model.GetType().Name;
			record.RelatedItemName = model.ToStringSafe();
			record.RelatedItemId = model.Id;
			record.ChangeType = changeType;

			return record;
		}

		#endregion
	}
}
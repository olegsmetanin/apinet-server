using System;
using System.Collections.Generic;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Security;

namespace AGO.Core.Model.Processing
{
	public abstract class CollectionChangeActivityPostProcessor<TModel, TRelatedModel> : ActivityPostProcessor<TModel>
		where TModel : SecureModel<Guid>, new()
		where TRelatedModel : SecureModel<Guid>
	{
		#region Properties, fields, constructors

		protected CollectionChangeActivityPostProcessor(
			ICrudDao crudDao,
			ISessionProvider sessionProvider)
			: base(crudDao, sessionProvider)
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

		protected virtual ActivityRecordModel PopulateCollectionActivityRecord(
			TModel model, 
			TRelatedModel relatedModel,
			CollectionChangeActivityRecordModel record, 
			ChangeType changeType)
		{
			record.ItemType = relatedModel.GetType().Name;
			record.ItemName = relatedModel.ToStringSafe();
			record.ItemId = relatedModel.Id;

			record.Creator = model.LastChanger;
			record.RelatedItemType = model.GetType().Name;
			record.RelatedItemName = model.ToStringSafe();
			record.RelatedItemId = model.Id;
			record.ChangeType = changeType;

			return record;
		}

		#endregion
	}
}
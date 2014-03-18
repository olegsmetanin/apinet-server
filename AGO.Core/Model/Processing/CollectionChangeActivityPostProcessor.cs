using System;
using System.Collections.Generic;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;

namespace AGO.Core.Model.Processing
{
	public abstract class CollectionChangeActivityPostProcessor<TModel, TRelatedModel> : ActivityPostProcessor<TModel>
		where TModel : IdentifiedModel<Guid>, new()
		where TRelatedModel : IdentifiedModel<Guid>
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
			ChangeType changeType,
			ProjectMemberModel member = null)
		{
			if (member == null && model is ISecureModel)
			{
				member = ((ISecureModel) model).LastChanger;
			}

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
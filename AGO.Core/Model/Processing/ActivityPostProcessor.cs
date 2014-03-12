using System;
using System.Collections.Generic;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Security;

namespace AGO.Core.Model.Processing
{
	public abstract class ActivityPostProcessor<TModel> : IModelPostProcessor
		where TModel : SecureModel<Guid>, new()
	{
		#region Properties, fields, constructors

		protected readonly ICrudDao _CrudDao;

		protected readonly ISessionProvider _SessionProvider;

		protected ActivityPostProcessor(
			ICrudDao crudDao,
			ISessionProvider sessionProvider)
		{
			if (crudDao == null)
				throw new ArgumentNullException("crudDao");
			_CrudDao = crudDao;

			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");
			_SessionProvider = sessionProvider;
		}

		#endregion

		#region Interfaces implementation

		public bool Accepts(IIdentifiedModel model)
		{
			return model is TModel;
		}

		public void AfterModelCreated(IIdentifiedModel model)
		{
			ProcessUpdate(model as TModel, new TModel());
		}

		public void AfterModelUpdated(IIdentifiedModel model, IIdentifiedModel original)
		{
			ProcessUpdate(model as TModel, original as TModel);
		}

		public void AfterModelDeleted(IIdentifiedModel model)
		{
			if (model == null)
				return;

			var records = RecordsForDeletion(model as TModel);
			if (records.Count == 0)
				return;

			foreach (var record in records)
				_CrudDao.Store(record);
		}

		#endregion

		#region Template methods

		protected virtual IList<ActivityRecordModel> RecordsForUpdate(TModel model, TModel original)
		{
			return new List<ActivityRecordModel>();
		}

		protected virtual IList<ActivityRecordModel> RecordsForDeletion(TModel model)
		{
			return new List<ActivityRecordModel>();
		}

		protected virtual ActivityRecordModel PopulateActivityRecord(TModel model, ActivityRecordModel record)
		{
			record.ItemType = model.GetType().Name;
			record.ItemName = model.ToStringSafe();
			record.ItemId = model.Id;
			record.Creator = model.LastChanger;

			return record;
		}

		#endregion

		#region Helper methods

		protected void ProcessUpdate(TModel model, TModel original)
		{
			if (model == null || original == null)
				return;

			var records = RecordsForUpdate(model, original);
			if (records.Count == 0)
				return;

			foreach (var record in records)
				_CrudDao.Store(record);
		}

		#endregion
	}
}
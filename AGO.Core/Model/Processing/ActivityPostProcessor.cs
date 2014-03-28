using System;
using System.Collections.Generic;
using AGO.Core.DataAccess;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;

namespace AGO.Core.Model.Processing
{
	public abstract class ActivityPostProcessor<TModel, TRecordModel> : IModelPostProcessor
		where TModel : IdentifiedModel<Guid>, new()
		where TRecordModel : ActivityRecordModel, new()
	{
		#region Properties, fields, constructors

		protected readonly DaoFactory DaoFactory;

		protected readonly ISessionProviderRegistry SessionProviderRegistry;

		protected ActivityPostProcessor(DaoFactory factory, ISessionProviderRegistry providerRegistry)
		{
			if (factory == null)
				throw new ArgumentNullException("factory");
			DaoFactory = factory;

			if (providerRegistry == null)
				throw new ArgumentNullException("providerRegistry");
			SessionProviderRegistry = providerRegistry;
		}

		#endregion

		#region Interfaces implementation

		public bool Accepts(IIdentifiedModel model)
		{
			return model is TModel;
		}

		public void AfterModelCreated(IIdentifiedModel model, ProjectMemberModel creator = null)
		{
			ProcessUpdate(model as TModel, new TModel(), creator);
		}

		public void AfterModelUpdated(IIdentifiedModel model, IIdentifiedModel original, ProjectMemberModel changer = null)
		{
			ProcessUpdate(model as TModel, original as TModel, changer);
		}

		public void AfterModelDeleted(IIdentifiedModel model, ProjectMemberModel deleter = null)
		{
			if (model == null)
				return;

			var records = RecordsForDeletion(model as TModel);
			if (records.Count == 0)
				return;

			foreach (var record in records)
				DaoForModel(model, deleter).Store(record);
		}

		protected virtual ICrudDao DaoForModel(IIdentifiedModel model, ProjectMemberModel changer)
		{
			if (changer == null && model is ISecureModel)
			{
				changer = ((ISecureModel)model).LastChanger ?? ((ISecureModel)model).Creator;
			}

			if (changer != null)
			{
				return DaoFactory.CreateProjectCrudDao(changer.ProjectCode);
			}

			var pm = model as IProjectBoundModel;
			if (pm != null)
			{
				return DaoFactory.CreateProjectCrudDao(pm.ProjectCode);
			}

			throw new InvalidOperationException("Can not determine project code for storing activity record");
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

		protected virtual TRecordModel PopulateActivityRecord(TModel model, TRecordModel record, ProjectMemberModel member = null)
		{
			if (member == null && model is ISecureModel)
				member = ((ISecureModel)model).LastChanger ?? ((ISecureModel)model).Creator;
			record.Creator = member;

			record.ItemType = model.GetType().Name;
			record.ItemName = model.ToStringSafe();
			record.ItemId = model.Id;		

			return record;
		}

		#endregion

		#region Helper methods

		protected void ProcessUpdate(TModel model, TModel original, ProjectMemberModel changer)
		{
			if (model == null || original == null)
				return;

			var records = RecordsForUpdate(model, original);
			if (records.Count == 0)
				return;

			foreach (var record in records)
				DaoForModel(model, changer).Store(record);
		}

		#endregion
	}
}
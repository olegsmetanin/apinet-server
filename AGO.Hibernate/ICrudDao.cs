using System;
using AGO.Hibernate.Model;

namespace AGO.Hibernate
{
	public interface ICrudDao
	{
		TModel Get<TModel>(
			object id,
			bool throwIfNotExist = false,
			Type modelType = null)
			where TModel : class, IIdentifiedModel;

		void Store(IIdentifiedModel model);

		void Delete(IIdentifiedModel model);

		TModel Refresh<TModel>(TModel model)
			where TModel : class, IIdentifiedModel;

		TModel Merge<TModel>(TModel model)
			where TModel : class, IIdentifiedModel;

		void CloseCurrentSession(bool forceRollback = false);
	}
}

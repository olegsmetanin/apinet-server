using System;
using AGO.Core.Model;

namespace AGO.Core
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

		void FlushCurrentSession(bool forceRollback = false);

		void CloseCurrentSession(bool forceRollback = false);
	}
}

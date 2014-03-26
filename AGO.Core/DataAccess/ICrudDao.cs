using System;
using System.Collections.Generic;
using AGO.Core.Filters;
using AGO.Core.Model;
using NHibernate;

namespace AGO.Core.DataAccess
{
	public interface ICrudDao
	{
		int MaxPageSize { get; }

		int DefaultPageSize { get; }

		ISessionProvider SessionProvider { get; }

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

		IList<TModel> List<TModel>(
			ICriteria criteria,
			FilteringOptions options = null)
			where TModel : class, IIdentifiedModel;

		IEnumerable<TModel> Future<TModel>(
			ICriteria criteria,
			FilteringOptions options = null)
			where TModel : class, IIdentifiedModel;

		int RowCount<TModel>(
			ICriteria criteria,
			Type modelType = null)
			where TModel : class, IIdentifiedModel;

		ICriteria PagedCriteria(ICriteria criteria, int page, int pageSize = 0);

		IQueryOver<TModel> PagedQuery<TModel>(IQueryOver<TModel> query, int page, int pageSize = 0)
			where TModel : class, IIdentifiedModel;

		IEnumerable<TModel> PagedFuture<TModel>(IQueryOver<TModel> query, int page, int pageSize = 0)
			where TModel : class, IIdentifiedModel;

		IList<TModel> PagedList<TModel>(IQueryOver<TModel> query, int page, int pageSize = 0)
			where TModel : class, IIdentifiedModel;

		IEnumerable<TModel> PagedFuture<TModel>(ICriteria criteria, int page, int pageSize = 0)
			where TModel : class, IIdentifiedModel;

		IList<TModel> PagedList<TModel>(ICriteria criteria, int page, int pageSize = 0)
			where TModel : class, IIdentifiedModel;
	}
}
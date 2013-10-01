using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Model;
using NHibernate;

namespace AGO.Core
{
	public interface ICrudDao
	{
		TModel Get<TModel>(
			object id,
			bool throwIfNotExist = false,
			Type modelType = null)
			where TModel : class, IIdentifiedModel;

		bool Exists<TModel>(IQueryOver<TModel> query) where TModel : class;

		bool Exists<TModel>(Func<IQueryOver<TModel, TModel>, IQueryOver<TModel, TModel>> query) where TModel : class;

		TModel Find<TModel>(Func<IQueryOver<TModel, TModel>, IQueryOver<TModel, TModel>> query) where TModel : class;

		void Store(IIdentifiedModel model);

		void Delete(IIdentifiedModel model);

		TModel Refresh<TModel>(TModel model)
			where TModel : class, IIdentifiedModel;

		TModel Merge<TModel>(TModel model)
			where TModel : class, IIdentifiedModel;

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

		void FlushCurrentSession(bool forceRollback = false);

		void CloseCurrentSession(bool forceRollback = false);
	}

	public static class CrudDaoExtensions
	{
		public static IQueryOver<TModel> PagedQuery<TModel>(
			this IQueryOver<TModel> query,
			ICrudDao crudDao,
			int page,
			int pageSize = 0)
			where TModel : class, IIdentifiedModel
		{
			return query != null && crudDao != null
				? crudDao.PagedQuery(query, page, pageSize)
				: query;
		}

		public static IEnumerable<TModel> PagedFuture<TModel>(
			this IQueryOver<TModel> query, 
			ICrudDao crudDao, 
			int page, 
			int pageSize = 0)
			where TModel : class, IIdentifiedModel
		{
			return query != null && crudDao != null 
				? crudDao.PagedFuture(query, page, pageSize)
				: Enumerable.Empty<TModel>();
		}

		public static IList<TModel> PagedList<TModel>(
			IQueryOver<TModel> query, 
			ICrudDao crudDao, 
			int page, 
			int pageSize = 0)
			where TModel : class, IIdentifiedModel
		{
			return query != null && crudDao != null
				? crudDao.PagedList(query, page, pageSize)
				: new List<TModel>();
		}
	}
}

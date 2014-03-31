using System;
using System.Collections.Generic;
using AGO.Core.Model;
using NHibernate;
using NHibernate.Criterion;

namespace AGO.Core.DataAccess
{
	public static class CrudDaoExtensions
	{
		public static IQueryOver<TModel> PagedQuery<TModel>(
			this ICrudDao crudDao,
			IQueryOver<TModel> query,
			int page,
			int pageSize = 0)
			where TModel : class, IIdentifiedModel
		{
			if (crudDao == null)
				throw new ArgumentNullException("crudDao");
			if (query == null)
				throw new ArgumentNullException("query");

			return crudDao.PagedQuery(query, page, pageSize);
		}

		public static IEnumerable<TModel> PagedFuture<TModel>(
			this ICrudDao crudDao,
			IQueryOver<TModel> query, 
			int page, 
			int pageSize = 0)
			where TModel : class, IIdentifiedModel
		{
			if (crudDao == null)
				throw new ArgumentNullException("crudDao");
			if (query == null)
				throw new ArgumentNullException("query");

			return crudDao.PagedFuture(query, page, pageSize);
		}

		public static IList<TModel> PagedList<TModel>(
			this ICrudDao crudDao, 
			IQueryOver<TModel> query, 
			int page, 
			int pageSize = 0)
			where TModel : class, IIdentifiedModel
		{
			if (crudDao == null)
				throw new ArgumentNullException("crudDao");
			if (query == null)
				throw new ArgumentNullException("query");

			return crudDao.PagedList(query, page, pageSize);
		}

		public static bool Exists<TModel>(
			this ICrudDao crudDao,
			Func<IQueryOver<TModel, TModel>, IQueryOver<TModel, TModel>> query) 
			where TModel : class
		{
			if (crudDao == null)
				throw new ArgumentNullException("crudDao");

			query = query ?? (q => q);
			return query(crudDao.SessionProvider.CurrentSession.QueryOver<TModel>()).Exists();
		}

		public static TModel Find<TModel>(
			this ICrudDao crudDao,
			Func<IQueryOver<TModel, TModel>, IQueryOver<TModel, TModel>> query) 
			where TModel : class
		{
			if (crudDao == null)
				throw new ArgumentNullException("crudDao");

			query = query ?? (q => q);
			return query(crudDao.SessionProvider.CurrentSession.QueryOver<TModel>()).SingleOrDefault();
		}
	}
}

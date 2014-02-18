using System;
using System.Collections.Generic;
using NHibernate;
using AGO.Core.Model;

namespace AGO.Core
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

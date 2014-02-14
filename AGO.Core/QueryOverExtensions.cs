using System;
using NHibernate;
using NHibernate.Criterion;

namespace AGO.Core
{
	public static class QueryOverExtensions
	{
		#region Existence checks

		public static bool Exists<TModel>(
			this IQueryOver<TModel> query) 
			where TModel : class
		{
			if (query == null)
				throw new ArgumentNullException("query");

			//Solution without havy count(*) operation, only
			//select top (1) 1 from xxx where...
			//May be more elegant way to write this in nhibernate exists
			return query.UnderlyingCriteria
				.SetProjection(Projections.Constant(1, NHibernateUtil.Int32))
				.SetMaxResults(1)
				.List<int>().Count > 0;
		}

		public static bool Exists<TModel>(
			this QueryOver<TModel> query,
			ISessionProvider sessionProvider) 
			where TModel : class
		{
			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");

			return query.GetExecutableQueryOver(sessionProvider.CurrentSession).Exists();
		}

		#endregion
	}
}
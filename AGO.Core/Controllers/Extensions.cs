using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AGO.Core.Filters;
using AGO.Core.Model;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace AGO.Core.Controllers
{
	public static class Extensions
	{
		//may be need later as example
//		public static IEnumerable<LookupEntry> LookupList<TModel, TAliasedModel>(
//			this IQueryOver<TModel> query,
//			Expression<Func<TModel, object>> idProperty,
//			string alias,
//			Expression<Func<TAliasedModel, object>> textProperty,
//			bool idToLowerCase = true)
//		{
//			if (query == null || idProperty == null)
//				return Enumerable.Empty<LookupEntry>();
//
//			query.UnderlyingCriteria
//				.SetProjection(Projections.ProjectionList()
//					.Add(Projections.Cast(NHibernateUtil.String, Projections.Property(idProperty)), "Id")
//					.Add(Projections.Cast(NHibernateUtil.String, 
//						Projections.Property(alias + "." + textProperty.PropertyInfoFromExpression().Name)), "Text")
//					)
//				.SetResultTransformer(Transformers.AliasToBean(typeof(LookupEntry)));
//
//			var result = query.List<LookupEntry>() as IEnumerable<LookupEntry>;
//			if (idToLowerCase)
//				result = result.Select(m => { m.Id = (m.Id ?? string.Empty).ToLower(); return m; });
//
//			return result;
//		}

		public static IEnumerable<LookupEntry> LookupList<TModel>(
			this IQueryOver<TModel> query,
			Expression<Func<TModel, object>> idProperty,
			Expression<Func<TModel, object>> textProperty = null,
			bool idToLowerCase = true)
		{
			if (query == null || idProperty == null)
				return Enumerable.Empty<LookupEntry>();

			return query.UnderlyingCriteria.LookupList(idProperty, textProperty, idToLowerCase);
		}

		public static IEnumerable<LookupEntry> LookupList<TModel>(
			this ICriteria criteria,
			Expression<Func<TModel, object>> idProperty,
			Expression<Func<TModel, object>> textProperty = null,
			bool idToLowerCase = true)
		{
			if (criteria == null || idProperty == null)
				return Enumerable.Empty<LookupEntry>();

			criteria
				.SetProjection(Projections.ProjectionList()
					.Add(Projections.Cast(NHibernateUtil.String, Projections.Property(idProperty)), "Id")
					.Add(Projections.Cast(NHibernateUtil.String, Projections.Property(textProperty ?? idProperty)), "Text"))
				.SetResultTransformer(Transformers.AliasToBean(typeof(LookupEntry)));

			var result = criteria.List<LookupEntry>() as IEnumerable<LookupEntry>;
			if (idToLowerCase)
				result = result.Select(m => { m.Id = (m.Id ?? string.Empty).ToLower(); return m; });

			return result;
		}

		public static IEnumerable<LookupEntry> LookupModelsList<TModel>(
			this IQueryOver<TModel> query,
			Expression<Func<TModel, object>> textProperty,
			bool idToLowerCase = true)
			where TModel : IIdentifiedModel<Guid>
		{
			return query.LookupList(m => m.Id, textProperty, idToLowerCase);
		}

		public static IEnumerable<LookupEntry> LookupModelsList<TModel>(
			this ICriteria criteria,
			Expression<Func<TModel, object>> textProperty,
			bool idToLowerCase = true)
			where TModel : IIdentifiedModel<Guid>
		{
			return criteria.LookupList(m => m.Id, textProperty, idToLowerCase);
		}
	}
}

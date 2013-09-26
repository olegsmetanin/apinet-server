using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AGO.Core.Model;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace AGO.Core.Controllers
{
	public static class Extensions
	{
		public static IEnumerable<LookupEntry> LookupList<TModel, TAliasedModel>(
			this IQueryOver<TModel> query,
			Expression<Func<TModel, object>> idProperty,
			string alias,
			Expression<Func<TAliasedModel, object>> textProperty)
		{
			if (query == null || idProperty == null)
				return Enumerable.Empty<LookupEntry>();

			query.UnderlyingCriteria
				.SetProjection(Projections.ProjectionList()
					.Add(Projections.Cast(NHibernateUtil.String, Projections.Property(idProperty)), "Id")
					.Add(Projections.Cast(NHibernateUtil.String, 
						Projections.Property(alias + "." + textProperty.PropertyInfoFromExpression().Name)), "Text")
					)
				.SetResultTransformer(Transformers.AliasToBean(typeof(LookupEntry)));

			return query.List<LookupEntry>();
		}

		public static IEnumerable<LookupEntry> LookupList<TModel>(
			this IQueryOver<TModel> query,
			Expression<Func<TModel, object>> idProperty,
			Expression<Func<TModel, object>> textProperty = null)
		{
			if (query == null || idProperty == null)
				return Enumerable.Empty<LookupEntry>();

			query.UnderlyingCriteria
				.SetProjection(Projections.ProjectionList()
					.Add(Projections.Cast(NHibernateUtil.String, Projections.Property(idProperty)), "Id")
					.Add(Projections.Cast(NHibernateUtil.String, Projections.Property(textProperty ?? idProperty)), "Text"))
				.SetResultTransformer(Transformers.AliasToBean(typeof(LookupEntry)));

			return query.List<LookupEntry>();
		}

		public static IEnumerable<LookupEntry> LookupModelsList<TModel>(
			this IQueryOver<TModel> query,
			Expression<Func<TModel, object>> textProperty)
			where TModel : IIdentifiedModel<Guid>
		{
			return query.LookupList(m => m.Id, textProperty);
		}
	}
}

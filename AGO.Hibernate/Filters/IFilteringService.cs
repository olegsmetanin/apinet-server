using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using AGO.Hibernate.Model;
using NHibernate.Criterion;

namespace AGO.Hibernate.Filters
{
	public interface IFilteringService
	{
		void ValidateFilter(IModelFilterNode node, Type modelType);

		IModelFilterNode ParseFilterFromJson(TextReader reader, Type validateForModelType = null);

		IModelFilterNode ParseFilterFromJson(string str, Type validateForModelType = null);

		IModelFilterNode ParseFilterFromJson(Stream stream, Type validateForModelType = null);

		string GenerateJsonFromFilter(IModelFilterNode node);

		DetachedCriteria CompileFilter(IModelFilterNode node, Type modelType);

		IModelFilterNode ConcatFilters(
			IEnumerable<IModelFilterNode> nodes,
			ModelFilterOperators op = ModelFilterOperators.And);

		IModelFilterBuilder<TModel, TModel> Filter<TModel>()
			where TModel : class, IIdentifiedModel;
	}

	public static class FilteringServiceExtensions
	{
		public static IList<TModel> List<TModel>(
			this IModelFilterBuilder<TModel, TModel> filter,
			IFilteringDao dao,
			string orderBy = null,
			bool ascending = true,
			int? skip = null,
			int? take = null)
			where TModel : class, IIdentifiedModel
		{
			if (filter == null)
				throw new ArgumentNullException("filter");
			if (dao == null)
				throw new ArgumentNullException("dao");

			return dao.List<TModel>(new[] { filter }, orderBy, ascending, skip, take);
		}

		public static IList<TModel> List<TModel, TValue>(
			this IModelFilterBuilder<TModel, TModel> filter,
			IFilteringDao dao,
			Expression<Func<TModel, TValue>> orderBy = null,
			bool ascending = true,
			int? skip = null,
			int? take = null)
			where TModel : class, IIdentifiedModel
		{
			if (filter == null)
				throw new ArgumentNullException("filter");
			if (dao == null)
				throw new ArgumentNullException("dao");

			var orderByName = string.Empty;
			if (orderBy != null)
				orderByName = orderBy.PropertyInfoFromExpression().Name;

			return dao.List<TModel>(new[] { filter }, orderByName, ascending, skip, take);
		}

		public static IEnumerable<TModel> Future<TModel, TValue>(
			this IModelFilterBuilder<TModel, TModel> filter,
			IFilteringDao dao,
			Expression<Func<TModel, TValue>> orderBy = null,
			bool ascending = true,
			int? skip = null,
			int? take = null)
			where TModel : class, IIdentifiedModel
		{
			if (filter == null)
				throw new ArgumentNullException("filter");
			if (dao == null)
				throw new ArgumentNullException("dao");

			var orderByName = string.Empty;
			if (orderBy != null)
				orderByName = orderBy.PropertyInfoFromExpression().Name;

			return dao.Future<TModel>(new[] { filter }, orderByName, ascending, skip, take);
		}

		public static int RowCount<TModel>(
			this IModelFilterBuilder<TModel, TModel> filter,
			IFilteringDao dao)
			where TModel : class, IIdentifiedModel
		{
			if (filter == null)
				throw new ArgumentNullException("filter");
			if (dao == null)
				throw new ArgumentNullException("dao");

			return dao.RowCount<TModel>(new[] { filter });
		}
	}
}
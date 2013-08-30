using System;
using System.Collections.Generic;
using System.IO;
using NHibernate.Criterion;
using AGO.Core.Model;

namespace AGO.Core.Filters
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
			FilteringOptions<TModel> options = null)
			where TModel : class, IIdentifiedModel
		{
			if (filter == null)
				throw new ArgumentNullException("filter");
			if (dao == null)
				throw new ArgumentNullException("dao");

			return dao.List<TModel>(new[] { filter }, options);
		}

		public static IEnumerable<TModel> Future<TModel>(
			this IModelFilterBuilder<TModel, TModel> filter,
			IFilteringDao dao,
			FilteringOptions<TModel> options = null)
			where TModel : class, IIdentifiedModel
		{
			if (filter == null)
				throw new ArgumentNullException("filter");
			if (dao == null)
				throw new ArgumentNullException("dao");

			return dao.Future<TModel>(new[] { filter }, options);
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
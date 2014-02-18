using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Model;

namespace AGO.Core.Filters
{
	public interface IFilteringDao
	{
		IFilteringService FilteringService { get; }

		IList<TModel> List<TModel>(
			IEnumerable<IModelFilterNode> filters,
			FilteringOptions options = null)
			where TModel : class, IIdentifiedModel;

		IEnumerable<TModel> Future<TModel>(
			IEnumerable<IModelFilterNode> filters,
			FilteringOptions options = null)
			where TModel : class, IIdentifiedModel;

		int RowCount<TModel>(
			IEnumerable<IModelFilterNode> filters,
			Type modelType = null)
			where TModel : class, IIdentifiedModel;
	}

	public static class FilteringDaoExtensions
	{
		public static IList<TModel> List<TModel>(
			this IFilteringDao filteringDao, 
			IEnumerable<IModelFilterNode> filters,
			int page = 0,
			ICollection<SortInfo> sorters = null) where TModel : class, IIdentifiedModel
		{
			if (filteringDao == null)
				throw new ArgumentNullException("filteringDao");

			return filteringDao.List<TModel>(filters, new FilteringOptions
			{
				Page = page,
				Sorters = sorters ?? Enumerable.Empty<SortInfo>().ToArray()
			});
		}

		public static IList<TModel> List<TModel>(
			this IFilteringDao filteringDao,
			IModelFilterNode filter,
			int page = 0,
			ICollection<SortInfo> sorters = null) where TModel : class, IIdentifiedModel
		{
			if (filteringDao == null)
				throw new ArgumentNullException("filteringDao");

			return filteringDao.List<TModel>(new [] {filter}, new FilteringOptions
			{
				Page = page,
				Sorters = sorters ?? Enumerable.Empty<SortInfo>().ToArray()
			});
		}

		public static IEnumerable<TModel> Future<TModel>(
			this IFilteringDao filteringDao,
			IEnumerable<IModelFilterNode> filters,
			int page = 0,
			ICollection<SortInfo> sorters = null) where TModel : class, IIdentifiedModel
		{
			if (filteringDao == null)
				throw new ArgumentNullException("filteringDao");

			return filteringDao.Future<TModel>(filters, new FilteringOptions
			{
				Page = page,
				Sorters = sorters ?? Enumerable.Empty<SortInfo>().ToArray()
			});
		}

		public static int RowCount<TModel>(
			this IFilteringDao filteringDao,
			IModelFilterNode filters) where TModel : class, IIdentifiedModel
		{
			if (filteringDao == null)
				throw new ArgumentNullException("filteringDao");

			return filteringDao.RowCount<TModel>(new [] {filters});
		}
	}
}

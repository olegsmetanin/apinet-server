using System;
using System.Collections.Generic;
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

		IList<TModel> List<TModel>(
			IEnumerable<IModelFilterNode> filters,
			int page, 
			ICollection<SortInfo> sorters)
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
}

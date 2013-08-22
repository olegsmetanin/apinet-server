using System;
using System.Collections.Generic;
using AGO.Hibernate.Filters.Metadata;
using AGO.Hibernate.Model;

namespace AGO.Hibernate.Filters
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
}

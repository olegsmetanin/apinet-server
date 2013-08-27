using System.Collections.Generic;
using AGO.Hibernate.Filters;

namespace AGO.Core.Json
{
	public interface IJsonModelsRequest : IJsonRequest
	{
		IList<IModelFilterNode> Filters { get; }

		IList<SortInfo> Sorters { get; }

		int Page { get; }

		int PageSize { get; }
	}
}

using System.Collections.Generic;
using AGO.Core.Filters;

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

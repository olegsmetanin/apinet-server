using System.Collections.Generic;
using AGO.Hibernate.Filters;

namespace AGO.Docstore.Json
{
	internal class JsonModelsRequest : JsonRequest, IJsonModelsRequest
	{
		private readonly IList<IModelFilterNode> _Filters = new List<IModelFilterNode>();
		public IList<IModelFilterNode> Filters { get { return _Filters; } }

		private readonly IList<SortInfo> _Sorters = new List<SortInfo>();
		public IList<SortInfo> Sorters { get { return _Sorters; } }

		public int Page { get; set; }

		public int PageSize { get; set; }
	}
}

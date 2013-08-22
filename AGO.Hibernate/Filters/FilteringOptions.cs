using System;
using System.Collections.Generic;

namespace AGO.Hibernate.Filters
{
	public enum FetchStrategy
	{
		Default,
		FetchRootReferences,
		DontFetchReferences
	}

	public class FilteringOptions
	{
		public struct SortInfo
		{
			public string Property { get; set; }

			public bool Descending { get; set; }
		}

		private readonly IList<SortInfo> _Sorters = new List<SortInfo>();
		public IList<SortInfo> Sorters { get { return _Sorters; } }

		public int? Skip { get; set; }

		public int? Take { get; set; }

		public Type ModelType { get; set; }

		public FetchStrategy FetchStrategy { get; set; }
	}
}

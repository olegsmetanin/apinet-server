using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using AGO.Core.Model;

namespace AGO.Core.Filters
{
	public enum FetchStrategy
	{
		Default,
		FetchRootReferences,
		DontFetchReferences
	}

	public struct SortInfo
	{
		[JsonProperty("property")]
		public string Property { get; set; }

		[JsonProperty("descending")]
		public bool Descending { get; set; }
	}

	public struct SortInfo<T> where T : class, IIdentifiedModel
	{
		public Expression<Func<T, object>> Property { get; set; }

		public bool Descending { get; set; }
	}

	public class FilteringOptions
	{
		private ICollection<SortInfo> _Sorters = new List<SortInfo>();
		public ICollection<SortInfo> Sorters
		{
			get { return _Sorters; } 
			set { _Sorters = value ?? _Sorters; }
		}

		public SortInfo Sorter
		{
			get { return _Sorters.FirstOrDefault(); } 
			set
			{
				_Sorters.Clear();
				_Sorters.Add(value);
			}
		}

		public virtual IEnumerable<SortInfo> ActualSorters { get { return _Sorters; } }

		public int Page { get; set; }

		public int PageSize { get; set; }

		public Type ModelType { get; set; }

		public FetchStrategy FetchStrategy { get; set; }
	}

	public class FilteringOptions<TModel> : FilteringOptions
		where TModel : class, IIdentifiedModel
	{
		private ICollection<SortInfo<TModel>> _GenericSorters = new List<SortInfo<TModel>>();
		public new ICollection<SortInfo<TModel>> Sorters
		{
			get { return _GenericSorters; } 
			set { _GenericSorters = value ?? _GenericSorters; }
		}

		public new SortInfo<TModel> Sorter
		{
			get { return _GenericSorters.FirstOrDefault(); }
			set
			{
				_GenericSorters.Clear();
				_GenericSorters.Add(value);
			}
		}

		public override IEnumerable<SortInfo> ActualSorters
		{
			get
			{
				return _GenericSorters.Select(s => new SortInfo
				{
					Property = s.Property.PropertyInfoFromExpression().Name,
					Descending = s.Descending
				}).ToList();
			}
		}
	}
}

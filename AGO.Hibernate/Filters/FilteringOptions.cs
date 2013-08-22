using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AGO.Hibernate.Model;

namespace AGO.Hibernate.Filters
{
	public enum FetchStrategy
	{
		Default,
		FetchRootReferences,
		DontFetchReferences
	}

		public struct SortInfo
		{
			public string Property { get; set; }

			public bool Descending { get; set; }
		}

	public struct SortInfo<T>
			where T : class, IIdentifiedModel
	{
		public Expression<Func<T, object>> Property { get; set; }

		public bool Descending { get; set; }
	}

	public class FilteringOptions
	{
		private readonly IList<SortInfo> _Sorters = new List<SortInfo>();
		public IList<SortInfo> Sorters { get { return _Sorters; } }

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

		public int? Skip { get; set; }

		public int? Take { get; set; }

		public Type ModelType { get; set; }

		public FetchStrategy FetchStrategy { get; set; }
	}

	public class FilteringOptions<TModel> : FilteringOptions
		where TModel : class, IIdentifiedModel
	{
		private readonly IList<SortInfo<TModel>> _GenericSorters = new List<SortInfo<TModel>>();
		public new IList<SortInfo<TModel>> Sorters { get { return _GenericSorters; } }

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

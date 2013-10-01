using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using AGO.Core.Filters;
using AGO.Core.Model;

namespace AGO.Core
{
	public class CrudDao : AbstractDao, ICrudDao, IFilteringDao
	{
		#region Properties, fields, constructors

		protected readonly IFilteringService _FilteringService;

		public CrudDao(
			ISessionProvider sessionProvider,
			IFilteringService filteringService)
			: base(sessionProvider)
		{
			if (filteringService == null)
				throw new ArgumentNullException("filteringService");
			_FilteringService = filteringService;
		}

		#endregion

		#region Interfaces implementation

		public IFilteringService FilteringService { get { return _FilteringService; } }

		public virtual TModel Get<TModel>(
			object id,
			bool throwIfNotExist = false,
			Type modelType = null)
			where TModel : class, IIdentifiedModel
		{
			if (id == null)
				throw new ArgumentNullException("id");

			var result = CurrentSession.Get(modelType ?? typeof(TModel), id) as TModel;
			if (result == null && throwIfNotExist)
				throw new ObjectNotFoundException(id, typeof(TModel));
			return result;
		}

		public bool Exists<TModel>(IQueryOver<TModel> query) where TModel : class
		{
			//Solution without havy count(*) operation, only
			//select top (1) 1 from xxx where...
			//May be more elegant way to write this in nhibernate
			return query.UnderlyingCriteria
				.SetProjection(Projections.Constant(1, NHibernateUtil.Int32))
				.SetMaxResults(1)
				.List<int>()
				.Count > 0;
		}

		public bool Exists<TModel>(Func<IQueryOver<TModel, TModel>, IQueryOver<TModel, TModel>> query) where TModel : class
		{
			var q = _SessionProvider.CurrentSession.QueryOver<TModel>();
			q = query(q);
			return Exists(q);
		}

		public TModel Find<TModel>(Func<IQueryOver<TModel, TModel>, IQueryOver<TModel, TModel>> query) where TModel : class
		{
			var q = _SessionProvider.CurrentSession.QueryOver<TModel>();
			q = query(q);
			return q.SingleOrDefault();
		}

		public virtual TModel Refresh<TModel>(TModel model)
			where TModel : class, IIdentifiedModel
		{
			if (model == null)
				throw new ArgumentNullException("model");

			if (!(model is IVirtualModel) && !model.IsNew())
				CurrentSession.Refresh(model);
			return model;
		}

		public virtual TModel Merge<TModel>(TModel model)
			where TModel : class, IIdentifiedModel
		{
			if (model == null)
				return null;
			var session = CurrentSession;

			if (_SessionProvider.SessionFactory.GetClassMetadata(model.RealType) != null && 
					!(model is IVirtualModel) && !model.IsNew())
				model = session.Merge(model);

			return model;
		}

		public IQueryOver<TModel> PagedQuery<TModel>(IQueryOver<TModel> query, int page, int pageSize = 0)
			where TModel : class, IIdentifiedModel
		{
			if (query == null)
				throw new ArgumentNullException("query");

			if (page < 0)
				page = 0;

			if (pageSize <= 0)
				pageSize = _DefaultPageSize;
			if (pageSize > _MaxPageSize)
				pageSize = _MaxPageSize;

			return query.Skip(page * pageSize).Take(pageSize);
		}

		public IEnumerable<TModel> PagedFuture<TModel>(IQueryOver<TModel> query, int page, int pageSize = 0)
			where TModel : class, IIdentifiedModel
		{
			if (query == null)
				throw new ArgumentNullException("query");

			return PagedQuery(query, page, pageSize).Future();
		}

		public IList<TModel> PagedList<TModel>(IQueryOver<TModel> query, int page, int pageSize = 0) 
			where TModel : class, IIdentifiedModel
		{
			return PagedFuture(query, page, pageSize).ToList();
		}

		public IEnumerable<TModel> PagedFuture<TModel>(ICriteria criteria, int page, int pageSize = 0) 
			where TModel : class, IIdentifiedModel
		{
			if (criteria == null)
				throw new ArgumentNullException("criteria");

			if (page < 0)
				page = 0;

			if (pageSize <= 0)
				pageSize = _DefaultPageSize;
			if (pageSize > _MaxPageSize)
				pageSize = _MaxPageSize;

			return criteria.SetFirstResult(page * pageSize)
				.SetMaxResults(pageSize)
				.Future<TModel>();
		}

		public IList<TModel> PagedList<TModel>(ICriteria criteria, int page, int pageSize = 0) 
			where TModel : class, IIdentifiedModel
		{
			return PagedFuture<TModel>(criteria, page, pageSize).ToList();
		}

		public virtual void Store(IIdentifiedModel model)
		{
			if (model == null)
				throw new ArgumentNullException("model");

			CurrentSession.SaveOrUpdate(model);
		}

		public virtual void Delete(IIdentifiedModel model)
		{
			if (model == null)
				throw new ArgumentNullException("model");

			CurrentSession.Delete(model);
		}

		public IList<TModel> List<TModel>(
			IEnumerable<IModelFilterNode> filters,
			FilteringOptions options = null)
			where TModel : class, IIdentifiedModel
		{
			return Future<TModel>(filters, options).ToList();
		}

		public IList<TModel> List<TModel>(IEnumerable<IModelFilterNode> filters, 
			int page = 0, ICollection<SortInfo> sorters = null) where TModel : class, IIdentifiedModel
		{
			var options = new FilteringOptions
			              	{
			              		Page = page,
			              		Sorters = sorters ?? Enumerable.Empty<SortInfo>().ToArray()
			              	};
			return List<TModel>(filters, options);
		}

		public IEnumerable<TModel> Future<TModel>(
			IEnumerable<IModelFilterNode> filters,
			FilteringOptions options = null) where TModel : class, IIdentifiedModel
		{
			if (filters == null)
				throw new ArgumentNullException("filters");

			options = options ?? new FilteringOptions();	

			var compiled = _FilteringService.CompileFilter(
				_FilteringService.ConcatFilters(filters), options.ModelType ?? typeof(TModel));

			var criteria = compiled.GetExecutableCriteria(CurrentSession);
			foreach (var sortInfo in options.ActualSorters.Where(sortInfo => !sortInfo.Property.IsNullOrWhiteSpace()))
				criteria = criteria.AddOrder(new Order(sortInfo.Property.TrimSafe(), !sortInfo.Descending));

			if (options.FetchStrategy == FetchStrategy.FetchRootReferences || options.FetchStrategy == FetchStrategy.DontFetchReferences)
			{
				var metadata = _SessionProvider.ModelMetadata(options.ModelType ?? typeof(TModel));
				if (metadata == null)
					throw new Exception("Requested model type is not mapped");

				foreach (var modelProperty in metadata.ModelProperties.Where(m => !m.IsCollection))
						criteria.SetFetchMode(modelProperty.Name, options.FetchStrategy == 
					FetchStrategy.FetchRootReferences ? FetchMode.Join : FetchMode.Lazy);
			}

			return PagedFuture<TModel>(criteria, options.Page, options.PageSize);
		}

		public int RowCount<TModel>(
			IEnumerable<IModelFilterNode> filters,
			Type modelType = null)
			where TModel : class, IIdentifiedModel
		{
			if (filters == null)
				throw new ArgumentNullException("filters");

			var compiled = _FilteringService.CompileFilter(
				_FilteringService.ConcatFilters(filters), modelType ?? typeof(TModel));

			return compiled.GetExecutableCriteria(CurrentSession)
				.SetProjection(Projections.RowCount())
				.UniqueResult<int>();
		}

		public void FlushCurrentSession(bool forceRollback = false)
		{
			_SessionProvider.FlushCurrentSession(forceRollback);
		}

		public void CloseCurrentSession(bool forceRollback = false)
		{
			_SessionProvider.CloseCurrentSession(forceRollback);
		}

		#endregion

		#region Template methods

		protected override void DoInitialize()
		{
			base.DoInitialize();

			_FilteringService.TryInitialize();
		}

		#endregion
	}
}

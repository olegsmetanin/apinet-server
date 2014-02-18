using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using AGO.Core.Filters;
using AGO.Core.Model;

namespace AGO.Core
{
	public class CrudDao : AbstractDao, ICrudDao, IFilteringDao
	{
		#region Configuration properties, fields and methods

		protected int _MaxPageSize = 100;

		protected int _DefaultPageSize = 20;

		protected override void DoSetConfigProperty(string key, string value)
		{
			if ("MaxPageSize".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				_MaxPageSize = value.ConvertSafe<int>();
			if ("DefaultPageSize".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				_DefaultPageSize = value.ConvertSafe<int>();
		}

		protected override string DoGetConfigProperty(string key)
		{
			if ("MaxPageSize".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				return _MaxPageSize.ToString(CultureInfo.InvariantCulture);
			if ("DefaultPageSize".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				return _DefaultPageSize.ToString(CultureInfo.InvariantCulture);
			return null;
		}

		#endregion

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

		public int MaxPageSize { get { return _MaxPageSize; } }

		public int DefaultPageSize { get { return _DefaultPageSize; } }

		public ISessionProvider SessionProvider { get { return _SessionProvider; } }

		public IFilteringService FilteringService { get { return _FilteringService; } }

		public TModel Get<TModel>(
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

		public TModel Refresh<TModel>(TModel model)
			where TModel : class, IIdentifiedModel
		{
			if (model == null)
				throw new ArgumentNullException("model");

			if (!(model is IVirtualModel) && !model.IsNew())
				CurrentSession.Refresh(model);
			return model;
		}

		public TModel Merge<TModel>(TModel model)
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

		public ICriteria PagedCriteria(ICriteria criteria, int page, int pageSize = 0)
		{
			if (criteria == null)
				throw new ArgumentNullException("criteria");

			if (page < 0)
				page = 0;

			if (pageSize <= 0)
				pageSize = _DefaultPageSize;
			if (pageSize > _MaxPageSize)
				pageSize = _MaxPageSize;

			return criteria.SetFirstResult(page * pageSize).SetMaxResults(pageSize);
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
			CurrentSession.FlushMode = FlushMode.Auto;
		}

		public virtual void Delete(IIdentifiedModel model)
		{
			if (model == null)
				throw new ArgumentNullException("model");

			CurrentSession.Delete(model);
			CurrentSession.FlushMode = FlushMode.Auto;
		}

		public IList<TModel> List<TModel>(
			IEnumerable<IModelFilterNode> filters,
			FilteringOptions options = null)
			where TModel : class, IIdentifiedModel
		{
			return Future<TModel>(filters, options).ToList();
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
			{
				var finalSortProperty = sortInfo.Property.TrimSafe();
				var parts = finalSortProperty.Split('.');
				if (parts.Length > 1)
				{
					var currentAlias = new StringBuilder();
					for (var i = 0; i < parts.Length - 1; i++)
					{
						var path = currentAlias.ToString();
						if (path.Length > 0)
							path += ".";
						path += parts[i];

						if (currentAlias.Length > 0)
							currentAlias.Append('_');
						currentAlias.Append(parts[i]);

						if (criteria.GetCriteriaByAlias(currentAlias.ToString()) == null)
							criteria.CreateAlias(path, currentAlias.ToString(), JoinType.LeftOuterJoin);

					}
					currentAlias.Append('.');
					currentAlias.Append(parts[parts.Length - 1]);
					finalSortProperty = currentAlias.ToString();
				}
				criteria = criteria.AddOrder(new Order(finalSortProperty, !sortInfo.Descending));
			}

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

		#endregion

		#region Template methods

		protected override void DoFinalizeConfig()
		{
			base.DoFinalizeConfig();

			if (_MaxPageSize <= 0)
				_MaxPageSize = MaxPageSize;
			if (_DefaultPageSize <= 0)
				_DefaultPageSize = DefaultPageSize;
		}

		protected override void DoInitialize()
		{
			base.DoInitialize();

			_FilteringService.TryInitialize();
		}

		#endregion
	}
}

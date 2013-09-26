using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using AGO.Core.Filters;
using AGO.Core.Model;

namespace AGO.Core
{
	public class CrudDao : AbstractService, ICrudDao, IFilteringDao
	{
		#region Configuration properties, fields and methods

		private const int DefaultMaxPageSize = 100;
		protected int _MaxPageSize = DefaultMaxPageSize;

		protected override void DoSetConfigProperty(string key, string value)
		{
			if ("MaxPageSize".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				_MaxPageSize = value.ConvertSafe<int>();
		}

		protected override string DoGetConfigProperty(string key)
		{
			if ("MaxPageSize".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				return _MaxPageSize.ToString(CultureInfo.InvariantCulture);
			return null;
		}

		#endregion

		#region Properties, fields, constructors

		protected ISessionProvider _SessionProvider;

		protected readonly IFilteringService _FilteringService;

		protected ISession CurrentSession { get { return _SessionProvider.CurrentSession; } }

		public CrudDao(
			ISessionProvider sessionProvider,
			IFilteringService filteringService)
		{
			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");
			_SessionProvider = sessionProvider;

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

		public IEnumerable<TModel> Future<TModel>(
			IEnumerable<IModelFilterNode> filters,
			FilteringOptions options = null) where TModel : class, IIdentifiedModel
		{
			if (filters == null)
				throw new ArgumentNullException("filters");

			options = options ?? new FilteringOptions();

			var skip = options.Skip ?? 0;
			var take = options.Take ?? 0;

			if (skip < 0)
				skip = 0;
			if (take <= 0 || take > _MaxPageSize)
				take = _MaxPageSize;

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

			return criteria.SetFirstResult(skip)
				.SetMaxResults(take)
				.Future<TModel>();
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

		protected override void DoFinalizeConfig()
		{
			base.DoFinalizeConfig();

			if (_MaxPageSize <= 0)
				_MaxPageSize = DefaultMaxPageSize;
		}

		protected override void DoInitialize()
		{
			base.DoInitialize();

			_SessionProvider.TryInitialize();
			_FilteringService.TryInitialize();
		}

		#endregion
	}
}

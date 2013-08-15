using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using AGO.Hibernate.Attributes.Model;
using AGO.Hibernate.Filters;
using AGO.Hibernate.Filters.Metadata;
using AGO.Hibernate.Model;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Metadata;

namespace AGO.Hibernate
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
			string orderBy = null,
			bool ascending = true,
			int? skip = null,
			int? take = null,
			Type modelType = null)
			where TModel : class, IIdentifiedModel
		{
			return Future<TModel>(filters, orderBy, ascending, skip, take, modelType).ToList();
		}

		public IEnumerable<TModel> Future<TModel>(
			IEnumerable<IModelFilterNode> filters, 
			string orderBy = null, 
			bool ascending = true,
			int? skip = null,
			int? take = null, 
			Type modelType = null) where TModel : class, IIdentifiedModel
		{
			if (filters == null)
				throw new ArgumentNullException("filters");

			skip = skip ?? 0;
			take = take ?? 0;

			if (skip < 0)
				skip = 0;
			if (take <= 0 || take > _MaxPageSize)
				take = _MaxPageSize;

			var compiled = _FilteringService.CompileFilter(
				_FilteringService.ConcatFilters(filters), modelType ?? typeof(TModel));

			var criteria = compiled.GetExecutableCriteria(CurrentSession);
			if (!orderBy.IsNullOrWhiteSpace())
				criteria = criteria.AddOrder(new Order(orderBy.TrimSafe(), ascending));

			return criteria.SetFirstResult(skip.Value)
				.SetMaxResults(take.Value)
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

		public void CloseCurrentSession(bool forceRollback = false)
		{
			_SessionProvider.CloseCurrentSession(forceRollback);
		}

		public IEnumerable<IModelMetadata> AllModelsMetadata()
		{
			var result = new List<IModelMetadata>();

			foreach (var pair in _SessionProvider.SessionFactory.GetAllClassMetadata())
			{
				var internalClassMeta = pair.Value;
				var mappedClass = internalClassMeta.GetMappedClass(EntityMode.Poco);
				if (mappedClass == null)
					continue;
				
				var classMeta = new ModelMetadata
				{
					Name = pair.Key,
					ModelType = mappedClass
				};
				result.Add(classMeta);

				
				for (var i = 0; i < internalClassMeta.PropertyNames.Length; i++)
				{
					if (internalClassMeta.IsVersioned && i == internalClassMeta.VersionProperty)
						continue;
					AddPropertyMeta(classMeta, internalClassMeta, mappedClass, internalClassMeta.PropertyNames[i]);				
				}

				if (internalClassMeta.HasIdentifierProperty)
					AddPropertyMeta(classMeta, internalClassMeta, mappedClass, internalClassMeta.IdentifierPropertyName);
			}

			return result;
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

			var initializable = _SessionProvider as IInitializable;
			if (initializable != null)
				initializable.Initialize();

			initializable = _FilteringService as IInitializable;
			if (initializable != null)
				initializable.Initialize();
		}

		#endregion

		#region Helper methods

		internal void AddPropertyMeta(ModelMetadata classMeta, IClassMetadata internalClassMeta, Type mappedClass, string propertyName)
		{
			var internalPropertyMeta = internalClassMeta.GetPropertyType(propertyName);
			if (internalPropertyMeta == null)
				return;

			var propertyInfo = mappedClass.GetProperty(propertyName);
			if (propertyInfo == null)
				return;

			PropertyMetadata propertyMeta;
			if (internalPropertyMeta.IsAssociationType)
			{
				var propertyType = propertyInfo.PropertyType;
				var isCollection = internalPropertyMeta.IsCollectionType;
				if (isCollection)
				{
					if (!propertyType.IsGenericType)
						throw new InvalidOperationException("Collection model property must be generic");
					propertyType = propertyType.GetGenericArguments()[0];
				}
				if (!typeof (IIdentifiedModel).IsAssignableFrom(propertyType))
					throw new InvalidOperationException("Model property must be IIdentifiedModel");

				propertyMeta = new ModelPropertyMetadata
				{
					IsCollection = isCollection,
					PropertyType = propertyType
				};
				classMeta._ModelProperties.Add((IModelPropertyMetadata)propertyMeta);
			}
			else
			{
				var propertyType = propertyInfo.PropertyType;
				if (propertyType.IsNullable())
					propertyType = propertyType.GetGenericArguments()[0];
				if (!propertyType.IsValueType && !typeof(string).IsAssignableFrom(propertyType))
					throw new InvalidOperationException("Property is not primitive");

				var isTimestamp = propertyInfo.GetCustomAttributes(typeof(TimestampAttribute), false).Length > 0 &&
					typeof(DateTime).IsAssignableFrom(propertyType);

				PrimitivePropertyMetadata primitiveMeta;
				propertyMeta = primitiveMeta = new PrimitivePropertyMetadata
				{
					PropertyType = propertyType,
					IsTimestamp = isTimestamp
				};

				if (propertyType.IsEnum)
				{
					var displayNamesAttribute = propertyInfo.GetCustomAttributes(
						typeof(EnumDisplayNamesAttribute), false).OfType<EnumDisplayNamesAttribute>().FirstOrDefault();

					var displayNamesDict = displayNamesAttribute != null
						? displayNamesAttribute.DisplayNames
						: new Dictionary<string, string>();

					primitiveMeta.PossibleValues = new Dictionary<string, string>();
					foreach (var name in Enum.GetNames(propertyType))
					{
						primitiveMeta.PossibleValues[name] = displayNamesDict.ContainsKey(name)
							? displayNamesDict[name].TrimSafe()
							: name;
					}
				}

				classMeta._PrimitiveProperties.Add(primitiveMeta);
			}

			propertyMeta.Name = propertyName;
			propertyMeta.DisplayName = propertyName;
			var displayNameAttribute = propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute), false)
				.OfType<DisplayNameAttribute>().FirstOrDefault();
			if (displayNameAttribute != null)
				propertyMeta.DisplayName = displayNameAttribute.DisplayName.TrimSafe();
		}
		
		#endregion
	}
}

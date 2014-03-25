using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Filters.Metadata;
using AGO.Core.Model;
using NHibernate.Metadata;
using NHibernate.Type;

namespace AGO.Core
{
	public class SessionFactoryBuilder : AbstractService, ISessionProvider
	{
		#region Configuration properties, fields and methods

		protected Configuration _HibernateConfiguration = new Configuration();
		public Configuration HibernateConfiguration
		{
			get{return _HibernateConfiguration;}
			set { _HibernateConfiguration = value ?? _HibernateConfiguration; }
		}

		protected const string DriverClassConfigKey = "connection.driver_class";
		public Type DriverClass
		{
			get { return GetConfigProperty(DriverClassConfigKey).ConvertSafe<Type>(); }
			set { SetConfigProperty(DriverClassConfigKey, value.AssemblyQualifiedName); }
		}

		protected const string CollectionTypeFactoryClassConfigKey = "collectiontype.factory_class";
		public Type CollectionTypeFactoryClass
		{
			get { return GetConfigProperty(CollectionTypeFactoryClassConfigKey).ConvertSafe<Type>(); }
			set { SetConfigProperty(CollectionTypeFactoryClassConfigKey, value.AssemblyQualifiedName); }
		}

		protected const string CurrentSessionContextClassConfigKey = "current_session_context_class";
		public Type CurrentSessionContextClass
		{
			get { return GetConfigProperty(CurrentSessionContextClassConfigKey).ConvertSafe<Type>(); }
			set { SetConfigProperty(CurrentSessionContextClassConfigKey, value.AssemblyQualifiedName); }
		}

		protected const string ConnectionStringConfigKey = "connection.connection_string";
		public string ConnectionString
		{
			get { return GetConfigProperty(ConnectionStringConfigKey); }
			set { SetConfigProperty(ConnectionStringConfigKey, value); }
		}

		protected const string IsolationConfigKey = "connection.isolation";
		public IsolationLevel Isolation
		{
			get { return GetConfigProperty(IsolationConfigKey).ConvertSafe<IsolationLevel>(); }
			set { SetConfigProperty(IsolationConfigKey, value.ToString()); }
		}

		protected const string CommandTimeoutConfigKey = "command_timeout";
		public int CommandTimeout
		{
			get { return GetConfigProperty(CommandTimeoutConfigKey).ConvertSafe<int>(); }
			set { SetConfigProperty(CommandTimeoutConfigKey, value.ToString(CultureInfo.InvariantCulture)); }
		}

		protected const string DialectConfigKey = "dialect";
		public Type Dialect
		{
			get { return GetConfigProperty(DialectConfigKey).ConvertSafe<Type>(); }
			set { SetConfigProperty(DialectConfigKey, value.AssemblyQualifiedName); }
		}

		protected const string PrepareSqlConfigKey = "prepare_sql";
		public bool PrepareSql
		{
			get { return GetConfigProperty(PrepareSqlConfigKey).ConvertSafe<bool>(); }
			set { SetConfigProperty(PrepareSqlConfigKey, value.ToString(CultureInfo.InvariantCulture)); }
		}

		protected FlushMode _DefaultFlushMode = FlushMode.Never;

		protected override void DoSetConfigProperty(string key, string value)
		{
			if ("DefaultFlushMode".Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				_DefaultFlushMode = value.ParseEnumSafe(FlushMode.Never);
				return;
			}

			key = key.ToLower();
			_HibernateConfiguration.SetProperty(key, value);
		}

		protected override string DoGetConfigProperty(string key)
		{
			if ("DefaultFlushMode".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				return _DefaultFlushMode.ToString();

			return _HibernateConfiguration.GetProperty(key);
		}

		#endregion

		#region Properties, fields, constructors

		protected ISessionFactory _SessionFactory;

		protected readonly IList<IModelMetadata> _AllModelsMetadata = new List<IModelMetadata>();

		#endregion

		#region Interfaces implementation

		public ISession CurrentSession
		{
			get
			{
				if (!_Ready)
					throw new ServiceNotInitializedException();

				try
				{
					if (CurrentSessionContext.HasBind(_SessionFactory))
						return _SessionFactory.GetCurrentSession();

					var session = _SessionFactory.OpenSession();
					session.FlushMode = _DefaultFlushMode;

					CurrentSessionContext.Bind(session);
					return _SessionFactory.GetCurrentSession();
				}
				catch (HibernateException e)
				{
					throw new DataAccessException(e);
				}
			}
		}

		public void FlushCurrentSession(bool forceRollback = false)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();

			try
			{
				if (!CurrentSessionContext.HasBind(_SessionFactory))
					return;

				var session = _SessionFactory.GetCurrentSession();
				if (session == null || !session.IsDirty())
					return;

				if (!forceRollback && session.FlushMode != FlushMode.Never)
					session.Flush();
				session.Clear();
			}
			catch (HibernateException e)
			{
				throw new DataAccessException(e);
			}
		}

		public void CloseCurrentSession(bool forceRollback = false)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();

			FlushCurrentSession(forceRollback);

			try
			{
				if (!CurrentSessionContext.HasBind(_SessionFactory))
					return;

				var session = _SessionFactory.GetCurrentSession();
				if (session != null)
					session.Close();

				CurrentSessionContext.Unbind(_SessionFactory);
			}
			catch (HibernateException e)
			{
				throw new DataAccessException(e);
			}
		}

		public ISessionFactory SessionFactory
		{
			get
			{
				if (!_Ready)
					throw new ServiceNotInitializedException();

				return _SessionFactory;
			}
		}

		public IEnumerable<IModelMetadata> AllModelsMetadata { get { return _AllModelsMetadata; } }
		
		public IModelMetadata ModelMetadata(Type modelType)
		{
			if (modelType == null)
				throw new ArgumentNullException("modelType");

			return _AllModelsMetadata.FirstOrDefault(m => modelType == m.ModelType);
		}

		#endregion

		#region Template methods

		protected override void DoFinalizeConfig()
		{
			base.DoFinalizeConfig();

			CurrentSessionContextClass = CurrentSessionContextClass ?? typeof (CallSessionContext); //typeof(ThreadStaticSessionContext); not usable when two sessionfactory on same thread
			CollectionTypeFactoryClass = CollectionTypeFactoryClass ?? typeof(Net4CollectionTypeFactory);
		}

		protected override void DoInitialize()
		{
			base.DoInitialize();

			DoBuildSessionFactory();
			if (_SessionFactory != null)
				DoCalculateMetadata();
		}

		protected virtual void DoBuildSessionFactory()
		{
			_SessionFactory = _HibernateConfiguration.BuildSessionFactory();
		}

		protected virtual void DoCalculateMetadata()
		{
			_AllModelsMetadata.Clear();

			foreach (var mappedClass in _SessionFactory.GetAllClassMetadata().Select(
					pair => pair.Value.GetMappedClass(EntityMode.Poco)).Where(mappedClass => mappedClass != null))
				PopulateClassMeta(mappedClass);
		}

		#endregion

		#region Helper methods

		internal void PopulateClassMeta(Type mappedClass)
		{			
			var currentClass = mappedClass;
			IClassMetadata internalClassMeta = null;

			while (currentClass != null)
			{
				var currentClassMeta = _SessionFactory.GetClassMetadata(currentClass);
				if (currentClassMeta == null)
					break;
				internalClassMeta = internalClassMeta ?? currentClassMeta;

				var excludeAttribute = currentClass.FirstAttribute<MetadataExcludeAttribute>(false);
				if (excludeAttribute != null)
					break;

				var classMeta = _AllModelsMetadata.OfType<ModelMetadata>().FirstOrDefault(m => m.ModelType == currentClass);
				if (classMeta == null)
				{
					classMeta = new ModelMetadata
					{
						Name = currentClassMeta.EntityName,
						ModelType = currentClass
					};

					_AllModelsMetadata.Add(classMeta);
				}

				for (var i = 0; i < internalClassMeta.PropertyNames.Length; i++)
				{
					if (internalClassMeta.IsVersioned && i == internalClassMeta.VersionProperty)
						continue;

					var propertyInfo = mappedClass.GetProperty(internalClassMeta.PropertyNames[i]);
					if (propertyInfo == null)
						continue;

					if (currentClass != mappedClass && currentClass.GetProperty(internalClassMeta.PropertyNames[i]) != null)
						continue;

					AddPropertyMeta(classMeta, internalClassMeta.GetPropertyType(internalClassMeta.PropertyNames[i]), propertyInfo,
						currentClass == mappedClass ? internalClassMeta.PropertyNames[i] : mappedClass.Name + "." + internalClassMeta.PropertyNames[i]);
				}

				if (currentClass == mappedClass && internalClassMeta.HasIdentifierProperty)
						AddPropertyMeta(classMeta, internalClassMeta.GetPropertyType(internalClassMeta.IdentifierPropertyName), 
					mappedClass.GetProperty(internalClassMeta.IdentifierPropertyName), internalClassMeta.IdentifierPropertyName);

				currentClass = currentClass.BaseType;
			}
		}

		internal void AddPropertyMeta(ModelMetadata classMeta, IType internalPropertyMeta, PropertyInfo propertyInfo, string propertyName)
		{
			if (internalPropertyMeta == null)
				return;

			var excludeAttribute = propertyInfo.FirstAttribute<MetadataExcludeAttribute>(false);
			if (excludeAttribute != null)
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
				if (!typeof(IIdentifiedModel).IsAssignableFrom(propertyType))
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

				classMeta._PrimitiveProperties.Add(primitiveMeta);
			}

			propertyMeta.Name = propertyName;
		}

		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using AGO.Hibernate.Attributes.Model;
using AGO.Hibernate.Filters.Metadata;
using AGO.Hibernate.Model;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Metadata;

namespace AGO.Hibernate
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

		protected override void DoSetConfigProperty(string key, string value)
		{
			key = key.ToLower();

			_HibernateConfiguration.SetProperty(key, value);
		}

		protected override string DoGetConfigProperty(string key)
		{
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

				if (CurrentSessionContext.HasBind(_SessionFactory))
					return _SessionFactory.GetCurrentSession();

				CurrentSessionContext.Bind(_SessionFactory.OpenSession());
				return _SessionFactory.GetCurrentSession();
			}
		}

		public void CloseCurrentSession(bool forceRollback = false)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();

			if (!CurrentSessionContext.HasBind(_SessionFactory))
				return;

			var session = _SessionFactory.GetCurrentSession();
			if (session != null)
			{
				var flushMode = session.FlushMode;
				if (!forceRollback && flushMode != FlushMode.Never && flushMode != FlushMode.Unspecified)
					session.Flush();
				session.Close();
			}

			CurrentSessionContext.Unbind(_SessionFactory);
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

			CurrentSessionContextClass = CurrentSessionContextClass ?? typeof(ThreadStaticSessionContext);
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

			foreach (var pair in _SessionFactory.GetAllClassMetadata())
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
				_AllModelsMetadata.Add(classMeta);

				
				for (var i = 0; i < internalClassMeta.PropertyNames.Length; i++)
				{
					if (internalClassMeta.IsVersioned && i == internalClassMeta.VersionProperty)
						continue;
					AddPropertyMeta(classMeta, internalClassMeta, mappedClass, internalClassMeta.PropertyNames[i]);				
				}

				if (internalClassMeta.HasIdentifierProperty)
					AddPropertyMeta(classMeta, internalClassMeta, mappedClass, internalClassMeta.IdentifierPropertyName);
			}
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
using System;
using System.Data;
using System.Globalization;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;

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
		}

		protected virtual void DoBuildSessionFactory()
		{
			_SessionFactory = _HibernateConfiguration.BuildSessionFactory();
		}

		#endregion
	}
}
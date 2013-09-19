using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using AGO.Core.AutoMapping;
using AGO.Core.Config;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Migration;
using SimpleInjector;

namespace AGO.Core.Application
{
	public abstract class AbstractApplication
	{
		#region Static properties

		protected static AbstractApplication _Current;
		public static AbstractApplication Current { get { return _Current; } }

		#endregion

		#region Properties, fields, constructors

		protected Container _Container;
		public Container Container { get { return _Container; } }

		protected IJsonService _JsonService;
		public IJsonService JsonService { get { return _JsonService; } }

		protected IFilteringService _FilteringService;
		public IFilteringService FilteringService { get { return _FilteringService; } }

		protected ISessionProvider _SessionProvider;
		public ISessionProvider SessionProvider { get { return _SessionProvider; } }

		protected IFilteringDao _FilteringDao;
		public IFilteringDao FilteringDao { get { return _FilteringDao; } }

		protected ICrudDao _CrudDao;
		public ICrudDao CrudDao { get { return _CrudDao; } }

		protected IMigrationService _MigrationService;
		public IMigrationService MigrationService { get { return _MigrationService; } }

		protected AbstractApplication()
		{
			_Current = this;
		}

		#endregion

		#region Registration

		protected abstract void Register();

		protected virtual void RegisterEnvironment()
		{
			_Container.RegisterSingle<IJsonService, JsonService>();
			_Container.RegisterSingle<IFilteringService, FilteringService>();
		}

		protected void RegisterPersistence()
		{
			_Container.RegisterSingle<ISessionProvider, AutoMappedSessionFactoryBuilder>();
			_Container.RegisterSingle<CrudDao, CrudDao>();
			_Container.Register<ICrudDao>(_Container.GetInstance<CrudDao>);
			_Container.Register<IFilteringDao>(_Container.GetInstance<CrudDao>);
			_Container.RegisterSingle<IMigrationService, MigrationService>();
		}

		#endregion

		#region Configuration

		protected virtual void Configure(IKeyValueProvider keyValueProvider)
		{
			ConfigureEnvironment(keyValueProvider);
			ConfigurePersistence(keyValueProvider);
		}

		protected void ConfigureEnvironment(IKeyValueProvider keyValueProvider)
		{
			_Container.RegisterInitializer<JsonService>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^Json_(.*)", keyValueProvider)).ApplyTo(service));
			_Container.RegisterInitializer<FilteringService>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^Filtering_(.*)", keyValueProvider)).ApplyTo(service));
		}

		protected virtual void ConfigurePersistence(IKeyValueProvider keyValueProvider)
		{
			_Container.RegisterInitializer<AutoMappedSessionFactoryBuilder>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^Hibernate_(.*)", keyValueProvider)).ApplyTo(service));
			_Container.RegisterInitializer<CrudDao>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^Dao_(.*)", keyValueProvider)).ApplyTo(service));
			_Container.RegisterInitializer<MigrationService>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^Hibernate_(.*)", keyValueProvider)).ApplyTo(service));
		}

		#endregion

		#region Initialization

		protected virtual void Initialize()
		{
			var initializedServices = new List<IInitializable>();

			foreach (var initializable in _Container.GetCurrentRegistrations().Where(r => r.Lifestyle == Lifestyle.Singleton)
				.Select(r => r.GetInstance() as IInitializable).Where(i => i != null))
			{
				initializedServices.Add(initializable);
				initializable.Initialize();
			}

			AfterSingletonsInitialized(initializedServices);
			AfterContainerInitialized(initializedServices);
		}

		protected virtual void AfterSingletonsInitialized(IList<IInitializable> initializedServices)
		{
		}

		protected virtual void AfterContainerInitialized(IList<IInitializable> initializedServices)
		{
		}

		protected virtual void InitializeEnvironment(IList<IInitializable> initializedServices)
		{
			_JsonService = _Container.GetInstance<IJsonService>();
			_FilteringService = _Container.GetInstance<IFilteringService>();
		}

		protected virtual void InitializePersistence(IList<IInitializable> initializedServices)
		{
			_SessionProvider = _Container.GetInstance<ISessionProvider>();
			_FilteringDao = _Container.GetInstance<IFilteringDao>();
			_CrudDao = _Container.GetInstance<ICrudDao>();
			_MigrationService = _Container.GetInstance<IMigrationService>();
		}

		#endregion

		#region Entry point

		public virtual void InitContainer()
		{
			if (_Container != null)
				return;
			_Container = GetContainer();

			Register();
			Configure(GetKeyValueProvider());
			Initialize();
		}

		protected virtual Container GetContainer()
		{
			return new Container(new ContainerOptions { AllowOverridingRegistrations = true });
		}

		protected virtual IKeyValueProvider GetKeyValueProvider()
		{
			return new AppSettingsKeyValueProvider();
		}

		#endregion

		#region Helper methods

		protected void ExecuteNonQuery(string script, IDbConnection connection)
		{
			var scripts = new List<string>();
			using (var reader = new StringReader(script))
			{
				var line = reader.ReadLine();
				var currentBatch = new StringBuilder();
				while (line != null)
				{
					line = line.TrimSafe();
					if (!"GO".Equals(line, StringComparison.InvariantCultureIgnoreCase))
						currentBatch.AppendLine(line);
					else
					{
						if (currentBatch.Length > 0)
							scripts.Add(currentBatch.ToString());
						currentBatch = new StringBuilder();
					}

					line = reader.ReadLine();
				}
			}

			foreach (var str in scripts)
			{
				var command = connection.CreateCommand();
				command.CommandText = str;
				command.CommandType = CommandType.Text;

				var rowsAffected = command.ExecuteNonQuery();
				if (rowsAffected >= 0)
					Console.WriteLine("Rows affected: {0}", rowsAffected);
			}

			Console.WriteLine("Batch complete ({0} scripts executed)", scripts.Count);
		}

		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.AutoMapping;
using AGO.Core.Config;
using AGO.Core.Controllers;
using AGO.Core.Execution;
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

		protected virtual void Register()
		{
			RegisterEnvironment();
			RegisterPersistence();
			RegisterActionExecution();
			RegisterControllers();
		}

		protected virtual IEnumerable<Type> AllActionParameterResolvers
		{
			get { return Enumerable.Empty<Type>(); }
		}

		protected virtual IEnumerable<Type> AllActionParameterTransformers
		{
			get 
			{ 
				return new[]
				{
					typeof (FilterParameterTransformer),
					typeof (JsonTokenParameterTransformer),
					typeof (AttributeValidatingParameterTransformer)
				}; 
			}
		}

		protected virtual IEnumerable<Type> AllActionResultTransformers
		{
			get { return Enumerable.Empty<Type>(); }
		}

		protected virtual void RegisterEnvironment()
		{
			_Container.RegisterSingle<IJsonService, JsonService>();
			_Container.RegisterSingle<IFilteringService, FilteringService>();
		}

		protected virtual void RegisterPersistence()
		{
			_Container.RegisterSingle<ISessionProvider, AutoMappedSessionFactoryBuilder>();
			_Container.RegisterSingle<CrudDao, CrudDao>();
			_Container.Register<ICrudDao>(_Container.GetInstance<CrudDao>);
			_Container.Register<IFilteringDao>(_Container.GetInstance<CrudDao>);
			_Container.RegisterSingle<IMigrationService, MigrationService>();
		}

		protected virtual void RegisterActionExecution()
		{
			_Container.RegisterAll<IActionParameterResolver>(AllActionParameterResolvers);
			_Container.RegisterAll<IActionParameterTransformer>(AllActionParameterTransformers);
			_Container.RegisterAll<IActionResultTransformer>(AllActionResultTransformers);
			_Container.RegisterSingle<IActionExecutor, ActionExecutor>();
		}

		protected virtual void RegisterControllers()
		{
			_Container.RegisterSingle<AuthController, AuthController>();
		}

		#endregion

		#region Configuration

		protected virtual void Configure(IKeyValueProvider keyValueProvider)
		{
			ConfigureEnvironment(keyValueProvider);
			ConfigurePersistence(keyValueProvider);
			ConfigureControllers(keyValueProvider);
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

		protected virtual void ConfigureControllers(IKeyValueProvider keyValueProvider)
		{
			_Container.RegisterInitializer<AuthController>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^Core_Auth_(.*)", keyValueProvider)).ApplyTo(service));
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
			InitializeEnvironment(initializedServices);
			InitializePersistence(initializedServices);
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
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AGO.Core.Security;
using Common.Logging;
using AGO.Core.Config;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Modules;
using SimpleInjector;

namespace AGO.Core.Application
{
	public abstract class AbstractApplication : IApplication
	{
		#region Static properties

		protected static AbstractApplication _Current;
		public static AbstractApplication Current { get { return _Current; } }

		#endregion

		#region Properties, fields, constructors

		private Container _IocContainer;
		public virtual Container IocContainer
		{
			get
			{
				_IocContainer = _IocContainer ?? 
					new Container(new ContainerOptions { AllowOverridingRegistrations = true });
				return _IocContainer;
			}
		}

		protected IKeyValueProvider _KeyValueProvider;
		public virtual IKeyValueProvider KeyValueProvider
		{
			get
			{
				_KeyValueProvider = _KeyValueProvider ?? new AppSettingsKeyValueProvider();
				return _KeyValueProvider;	
			}
			set { _KeyValueProvider = value; }
		}

		private IList<IModuleDescriptor> _ModuleDescriptors;
		public ISecurityService SecurityService { get; private set; }

		public virtual IList<IModuleDescriptor> ModuleDescriptors
		{
			get
			{
				_ModuleDescriptors = _ModuleDescriptors ?? new List<IModuleDescriptor>(
					AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).SelectMany(a => a.GetExportedTypes()
						.Where(t => t.IsClass && t.IsPublic && typeof(IModuleDescriptor).IsAssignableFrom(t)))
						.Select(Activator.CreateInstance).OfType<IModuleDescriptor>()
						.OrderBy(m => m.Priority));

				return _ModuleDescriptors;
			}
		}

		public IEnvironmentService EnvironmentService { get; private set; }

		public IJsonService JsonService { get; private set; }

		public ILocalizationService LocalizationService { get; private set; }

		protected AbstractApplication()
		{
			_Current = this;
		}

		#endregion

		#region Interfaces implementation

		public void Initialize()
		{
			try
			{
				DoRegisterApplication();
				DoRegisterModules(ModuleDescriptors);

				DoInitializeSingletons();

				DoInitializeApplication();
				DoInitializeModules(ModuleDescriptors);
			}
			catch (Exception e)
			{
				LogManager.GetLogger(GetType()).Fatal(e.Message, e);
				throw;
			}
		}

		#endregion

		#region Template methods

		protected virtual void DoRegisterApplication()
		{
			DoRegisterCoreServices();
		}

		protected virtual void DoRegisterModules(IList<IModuleDescriptor> moduleDescriptors)
		{
			foreach (var moduleDescriptor in moduleDescriptors)
			{
				moduleDescriptor.Register(this);

				foreach (var serviceDescriptor in moduleDescriptor.Services.OrderBy(s => s.Priority))
					serviceDescriptor.Register(this);
			}
		}

		protected virtual void DoRegisterCoreServices()
		{
			IocContainer.RegisterSingle<IEnvironmentService, LocalEnvironmentService>();
			IocContainer.RegisterInitializer<LocalEnvironmentService>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^Environment_(.*)", KeyValueProvider)).ApplyTo(service));

			IocContainer.RegisterSingle<IJsonService, JsonService>();
			IocContainer.RegisterInitializer<JsonService>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^Json_(.*)", KeyValueProvider)).ApplyTo(service));

			IocContainer.RegisterSingle<ILocalizationService, LocalizationService>();
			IocContainer.RegisterInitializer<LocalizationService>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^Localization_(.*)", KeyValueProvider)).ApplyTo(service));

			IocContainer.RegisterSingle<ISecurityService, SecurityService>();
		}

		protected virtual void DoInitializeApplication()
		{
			DoInitializeCoreServices();
		}

		protected virtual void DoInitializeModules(IList<IModuleDescriptor> moduleDescriptors)
		{
			foreach (var moduleDescriptor in moduleDescriptors)
			{
				moduleDescriptor.Initialize(this);

				foreach (var serviceDescriptor in moduleDescriptor.Services.OrderBy(s => s.Priority))
					serviceDescriptor.Initialize(this);
			}
		}

		protected virtual void DoInitializeCoreServices()
		{
			EnvironmentService = IocContainer.GetInstance<IEnvironmentService>();
			DoSetDataDirectory();

			LocalizationService = IocContainer.GetInstance<ILocalizationService>();
			JsonService = IocContainer.GetInstance<IJsonService>();
			SecurityService = IocContainer.GetInstance<ISecurityService>();
		}

		protected virtual void DoInitializeSingletons()
		{
			foreach (var initializable in IocContainer.GetCurrentRegistrations().Where(r => r.Lifestyle == Lifestyle.Singleton)
					.Select(r => r.GetInstance() as IInitializable).Where(i => i != null))
				initializable.Initialize();
		}

		protected virtual void DoSetDataDirectory()
		{
			AppDomain.CurrentDomain.SetData("DataDirectory",
				Path.Combine(EnvironmentService.ApplicationAssembliesPath, "../App_Data"));
		}

		#endregion
	}
}
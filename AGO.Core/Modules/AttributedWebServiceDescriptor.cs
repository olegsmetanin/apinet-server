using System;
using System.Reflection;
using AGO.Core.Application;
using AGO.Core.Config;
using AGO.Core.Modules.Attributes;

namespace AGO.Core.Modules
{
	public class AttributedWebServiceDescriptor<TInterface, TService> : IWebServiceDescriptor	
		where TInterface : class
		where TService : class, TInterface, IKeyValueConfigurable
	{
		#region Properties, field, constructors

		public IModuleDescriptor Module { get; private set; }

		public Type ServiceType { get { return typeof(TInterface); } }

		public string Name { get; private set; }

		public int Priority { get; private set; }

		public AttributedWebServiceDescriptor(IModuleDescriptor module, string name = null, int priority = 0)
		{
			if (module == null)
				throw new ArgumentNullException("module");

			Module = module;
			Name = name.TrimSafe();
			if (Name.IsNullOrEmpty())
				Name = typeof(TService).Name.RemoveSuffix("Controller");
			Priority = priority;
		}

		#endregion

		#region Interfaces implementation

		public void Register(IApplication app)
		{
		}

		public void Initialize(IApplication app)
		{
		}

		public void RegisterWeb(IWebApplication app)
		{
			app.IocContainer.RegisterSingle<TInterface, TService>();
			app.IocContainer.RegisterInitializer<TService>(service => new KeyValueConfigProvider(
				new RegexKeyValueProvider(string.Format("^{0}_{1}_(.*)", 
					Module.Alias ?? Module.Name, Name), app.KeyValueProvider)).ApplyTo(service));
		}

		public void InitializeWeb(IWebApplication app)
		{
			foreach (var methodInfo in typeof(TService).GetMethods(BindingFlags.Instance | BindingFlags.Public))
			{
				var jsonEndpointAttribute = methodInfo.FirstAttribute<JsonEndpointAttribute>(true);
				if (jsonEndpointAttribute == null)
					continue;

				app.RegisterJsonEndpoint(this, methodInfo);
			}
		}

		#endregion
	}

	public class AttributedWebServiceDescriptor<TService> : AttributedWebServiceDescriptor<TService, TService>
		where TService : class, IKeyValueConfigurable
	{
		public AttributedWebServiceDescriptor(IModuleDescriptor module, string name = null, int priority = 0) 
			: base(module, name, priority)
		{
		}
	}
}

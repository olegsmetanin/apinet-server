using System;
using System.Reflection;
using AGO.Hibernate.Config;
using AGO.Hibernate.Modules.Attributes;

namespace AGO.Hibernate.Modules
{
	public class AttributedServiceDescriptor<TInterface, TService> : IServiceDescriptor	
		where TInterface : class
		where TService : class, TInterface, IKeyValueConfigurable
	{
		public IModuleDescriptor Module { get; private set; }

		public Type ServiceType { get { return typeof(TInterface); } }

		public string Name { get; private set; }

		public int Priority { get; private set; }

		public void Register(IModuleConsumer consumer)
		{
			consumer.Container.RegisterSingle<TInterface, TService>();
			consumer.Container.RegisterInitializer<TService>(service => new KeyValueConfigProvider(
				new RegexKeyValueProvider(string.Format("^Service{0}_(.*)", Name), consumer.KeyValueProvider)).ApplyTo(service));

			foreach (var methodInfo in typeof(TService).GetMethods(BindingFlags.Instance | BindingFlags.Public))
			{
				var jsonEndpointAttribute = methodInfo.FirstAttribute<JsonEndpointAttribute>(true);
				if (jsonEndpointAttribute == null)
					continue;

				consumer.RegisterJsonEndpoint(this, methodInfo);
			}
		}

		public AttributedServiceDescriptor(IModuleDescriptor module, string name = null, int priority = 0)
		{
			if (module == null)
				throw new ArgumentNullException("module");

			Module = module;
			Name = name.TrimSafe();
			if (Name.IsNullOrEmpty())
				Name = typeof(TService).Name.RemoveSuffix("Controller");
			Priority = priority;
		}
	}

	public class AttributedServiceDescriptor<TService> : AttributedServiceDescriptor<TService, TService>
		where TService : class, IKeyValueConfigurable
	{
		public AttributedServiceDescriptor(IModuleDescriptor module, string name = null, int priority = 0) 
			: base(module, name, priority)
		{
		}
	}
}

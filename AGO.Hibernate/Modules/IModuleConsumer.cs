using System.Reflection;
using AGO.Hibernate.Config;
using SimpleInjector;

namespace AGO.Hibernate.Modules
{
	public interface IModuleConsumer
	{
		Container Container { get; }

		IKeyValueProvider KeyValueProvider { get; }

		void RegisterJsonEndpoint(IServiceDescriptor descriptor, MethodInfo method);
	}
}

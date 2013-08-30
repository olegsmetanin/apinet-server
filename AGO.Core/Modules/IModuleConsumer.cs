using System.Reflection;
using AGO.Core.Config;
using SimpleInjector;

namespace AGO.Core.Modules
{
	public interface IModuleConsumer
	{
		Container Container { get; }

		IKeyValueProvider KeyValueProvider { get; }

		void RegisterJsonEndpoint(IServiceDescriptor descriptor, MethodInfo method);
	}
}

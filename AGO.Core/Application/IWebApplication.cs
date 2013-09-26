using System.Reflection;
using AGO.Core.Modules;

namespace AGO.Core.Application
{
	public interface IWebApplication : IApplication
	{
		bool WebEnabled { get; set; }

		void RegisterJsonEndpoint(IServiceDescriptor descriptor, MethodInfo method);
	}
}
using AGO.Core.Application;

namespace AGO.Core.Modules
{
	public interface IWebServiceDescriptor : IServiceDescriptor
	{
		void RegisterWeb(IWebApplication app);

		void InitializeWeb(IWebApplication app);
	}
}

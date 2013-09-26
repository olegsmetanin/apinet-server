using AGO.Core.Controllers;

namespace AGO.Core.Application
{
	public interface IControllersApplication : IApplication
	{
		IStateStorage StateStorage { get; }
	}
}
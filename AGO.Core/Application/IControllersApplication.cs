using AGO.Core.Controllers;

namespace AGO.Core.Application
{
	public interface IControllersApplication : IApplication
	{
		IStateStorage<object> StateStorage { get; }

		IStateStorage<string> ClientStateStorage { get; } 
	}
}
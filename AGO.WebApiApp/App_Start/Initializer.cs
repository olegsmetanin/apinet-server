using AGO.WebApiApp.Application;
using WebActivator;
using Initializer = AGO.WebApiApp.App_Start.Initializer;

[assembly: PostApplicationStartMethod(typeof(Initializer), "Initialize")]

namespace AGO.WebApiApp.App_Start
{
	public static class Initializer
	{
		public static void Initialize()
		{
			new WebApplication { WebEnabled = true }.Initialize();
		}
	}
}
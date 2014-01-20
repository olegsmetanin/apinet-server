using System.Configuration;
using AGO.WebApiApp.Application;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(AGO.WebApiApp.OwinStartup))]

namespace AGO.WebApiApp
{
	public class OwinStartup
	{
		private WebApplication webapp;

		public void Configuration(IAppBuilder app)
		{
			webapp = new WebApplication { WebEnabled = true };
			webapp.Initialize();
		}
	}
}

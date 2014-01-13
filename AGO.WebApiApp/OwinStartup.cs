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
			//TODO replace with new redis startup
			//AGO.Notifications.Startup.StartupAsNotificationHost(app, ConfigurationManager.AppSettings["Hibernate_connection.connection_string"]);

			webapp = new WebApplication { WebEnabled = true };
			webapp.Initialize();
		}
	}
}

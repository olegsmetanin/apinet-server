using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(AGO.Reporting.Service.OwinStartup))]

namespace AGO.Reporting.Service
{
	public class OwinStartup
	{
		private ReportingService service;

		public void Configuration(IAppBuilder app)
		{
			//TODO extract from isessionprovider
			AGO.Notifications.Startup.StartupAsPublisher(ConfigurationManager.AppSettings["Hibernate_connection.connection_string"]);
			//AGO.Notifications.Startup.StartupAsNotificationHost(app, ConfigurationManager.AppSettings["Hibernate_connection.connection_string"]);

			service = new ReportingService();
			service.Initialize();
		}
	}
}

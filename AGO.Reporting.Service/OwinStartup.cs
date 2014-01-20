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
			service = new ReportingService();
			service.Initialize();
		}
	}
}

using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Routing;
using AGO.WebApiApp.App_Start;

namespace AGO.WebApiApp
{
	public enum DevMode
	{
		Dev,
		Prod
	}

	public class WebApiApplication : HttpApplication
	{
		public static DevMode DevMode { get; private set; }

		protected void Application_Start()
		{
			var config = WebConfigurationManager.OpenWebConfiguration("~/Web.config");
			var setting = config.AppSettings.Settings["DevMode"];
			if (setting != null)
			{
				DevMode result;
				if (Enum.TryParse(setting.Value, out result))
					DevMode = result;
			}

			WebApiConfig.Register(GlobalConfiguration.Configuration);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
		}
	}
}
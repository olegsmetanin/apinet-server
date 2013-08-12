using System;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using AGO.Hibernate;
using AGO.Hibernate.Application;
using AGO.Hibernate.Config;
using AGO.WebApiApp.App_Start;
using Common.Logging;
using SimpleInjector.Integration.Web.Mvc;
using WebActivator;

[assembly: PostApplicationStartMethod(typeof(Initializer), "Initialize")]

namespace AGO.WebApiApp.App_Start
{
	public enum DevMode
	{
		Dev,
		Prod
	}

	public class WebApplication : AbstractApplication
	{
		public static DevMode DevMode { get; private set; }

		protected override void Register()
		{
			var config = WebConfigurationManager.OpenWebConfiguration("~/Web.config");
			var setting = config.AppSettings.Settings["DevMode"];
			if (setting != null)
			{
				DevMode result;
				if (Enum.TryParse(setting.Value, out result))
					DevMode = result;
			}

			RegisterEnvironment();
			RegisterPersistence();
		}

		protected override IKeyValueProvider GetKeyValueProvider()
		{
			return new AppSettingsKeyValueProvider(
				WebConfigurationManager.OpenWebConfiguration("~/Web.config"));
		}

		protected override void AfterSingletonsInitialized(IList<IInitializable> initializedServices)
		{
			InitializeEnvironment(initializedServices);
			InitializePersistence(initializedServices);

			DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(_Container));

			RegisterWebApi(GlobalConfiguration.Configuration);
			RegisterRoutes(RouteTable.Routes);
		}

		protected override void DoMigrateUp()
		{
			base.DoMigrateUp();
			/*if (_MigrationService != null)
				_MigrationService.MigrateDown(new Version(1, 0, 0, 0));*/
		}

		protected void RegisterWebApi(HttpConfiguration config)
		{
			config.Routes.MapHttpRoute(
				"DefaultApi", 
				"api/{controller}/{id}", 
				new { id = RouteParameter.Optional });
		}

		protected void RegisterRoutes(RouteCollection routes)
		{
			routes.RouteExistingFiles = true;

			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Home",
				"",
				new { controller = "Home", action = "Index" });

			routes.MapRoute(
				"ng-app", 
				"{*path}", 
				new { controller = "StaticFiles", action = "StaticFile" });
		}
	}

	public static class Initializer
	{
		public static void Initialize()
		{
			try
			{
				new WebApplication().InitContainer();
			}
			catch (Exception e)
			{
				LogManager.GetLogger(typeof(WebApplication)).Fatal(e.Message, e);
			}
		}
	}
}
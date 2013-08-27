using System;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using AGO.Core.Json;
using AGO.Hibernate;
using AGO.Hibernate.Application;
using AGO.Hibernate.Config;
using AGO.Hibernate.Modules;
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

	public class WebApplication : AbstractApplication, IModuleConsumer
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

			_Container.RegisterSingle<IJsonRequestService, JsonRequestService>();
		}

		public IKeyValueProvider KeyValueProvider { get; private set; }

		public void RegisterJsonEndpoint(IServiceDescriptor descriptor, MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			var moduleName = !descriptor.Module.Alias.IsNullOrWhiteSpace()
				? descriptor.Module.Alias.TrimSafe() : descriptor.Module.Name.TrimSafe();
			if (moduleName.IsNullOrEmpty())
				throw new Exception(string.Format("Empty module name in \"{0}\"", descriptor.Module.GetType().AssemblyQualifiedName));

			var serviceName = descriptor.Name.TrimSafe();
			if (serviceName.IsNullOrEmpty())
				throw new Exception(string.Format("Empty service name in \"{0}\"", descriptor.GetType().AssemblyQualifiedName));

			var routeName = string.Format("{0}_{1}_{2}",
			    descriptor.Module.Name, descriptor.Name, method).Replace('.', '_');
			var routePath = string.Format("api/{0}/{1}/{2}", 
				moduleName.FirstCharToLower(), 
				serviceName.FirstCharToLower(),
				method.Name.FirstCharToLower());

			RouteTable.Routes.MapRoute(
				routeName,
				routePath,
				new { controller = "Api", action = "Dispatch", serviceType = descriptor.ServiceType, method });
		}

		protected override IKeyValueProvider GetKeyValueProvider()
		{
			KeyValueProvider = new AppSettingsKeyValueProvider(
				WebConfigurationManager.OpenWebConfiguration("~/Web.config"));
			return KeyValueProvider;
		}

		protected void RegisterCoreRoutes(RouteCollection routes)
		{
			routes.RouteExistingFiles = true;

			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Home",
				"",
				new { controller = "Home", action = "Index" });

			routes.MapRoute(
				"AllModelsMetadata",
				"metadata/AllModelsMetadata",
				new { controller = "Metadata", action = "AllModelsMetadata" });
		}

		protected void RegisterDefaultRoute(RouteCollection routes)
		{
			RouteTable.Routes.MapRoute(
				"ng-app",
				"{*path}",
				new { controller = "StaticFiles", action = "StaticFile" });
		}

		protected void RegisterModules()
		{
			foreach (var moduleDescriptor in AppDomain.CurrentDomain.GetAssemblies()
				.Where(a => !a.IsDynamic)
				.SelectMany(a => a.GetExportedTypes()
					.Where(t => t.IsClass && t.IsPublic && typeof(IModuleDescriptor).IsAssignableFrom(t)))
					.Select(Activator.CreateInstance).OfType<IModuleDescriptor>()
					.OrderBy(m => m.Priority))
			{
				foreach (var serviceDescriptor in moduleDescriptor.Services.OrderBy(s => s.Priority))
					serviceDescriptor.Register(this);
			}
		}

		protected override void Initialize()
		{
			RegisterCoreRoutes(RouteTable.Routes);

			RegisterModules();



			RegisterDefaultRoute(RouteTable.Routes);		

			DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(_Container));
		}

		protected override void DoMigrateUp()
		{
			base.DoMigrateUp();
			/*if (_MigrationService != null)
				_MigrationService.MigrateDown(new Version(1, 0, 0, 0));*/
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
				throw new Exception("Fatal initialization exception");
			}
		}
	}
}
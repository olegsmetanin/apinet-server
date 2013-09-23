using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using AGO.Core;
using AGO.Core.Application;
using AGO.Core.Config;
using AGO.Core.Modules;
using AGO.WebApiApp.App_Start;
using AGO.WebApiApp.Execution;
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
			base.Register();

			DevMode = GetKeyValueProvider().Value("DevMode").ParseEnumSafe(DevMode.Dev);
		}

		protected override IEnumerable<Type> AllActionParameterResolvers
		{
			get { return base.AllActionParameterResolvers.Concat(
				new[] {typeof (JsonBodyParameterResolver)}); }
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
				"ProjectPage",
				"projects/{project}",
				new { controller = "Home", action = "Project" });

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

			base.Initialize();

			RegisterDefaultRoute(RouteTable.Routes);		

			DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(_Container));
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
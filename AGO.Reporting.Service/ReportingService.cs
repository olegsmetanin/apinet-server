using System;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using AGO.Core.Application;
using AGO.Core.Config;
using AGO.Core.Execution;
using AGO.Core.Model.Reporting;
using AGO.Reporting.Common;
using AGO.Reporting.Service;
using SimpleInjector.Integration.Web.Mvc;
using WebActivator;

[assembly: PostApplicationStartMethod(typeof(Initializer), "Initialize")]

namespace AGO.Reporting.Service
{
	public class ReportingService: AbstractPersistenceApplication, IReportingService
	{
		#region Configuration and initialization

		public override IKeyValueProvider KeyValueProvider
		{
			get
			{
				_KeyValueProvider = _KeyValueProvider ?? new AppSettingsKeyValueProvider(
					WebConfigurationManager.OpenWebConfiguration("~/Web.config"));
				return _KeyValueProvider;
			}
			set { base.KeyValueProvider = value; }
		}

		protected override void DoRegisterCoreServices()
		{
			base.DoRegisterCoreServices();

			IocContainer.Register<IReportingRepository, ReportingRepository>();
			IocContainer.RegisterSingle<IActionExecutor, ActionExecutor>();
		}

		protected override void DoInitializeCoreServices()
		{
			base.DoInitializeCoreServices();
//			if (!WebEnabled)
//				return;

			RegisterReportingRoutes(RouteTable.Routes);
			DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(IocContainer));
		}

		protected void RegisterReportingRoutes(RouteCollection routes)
		{
			routes.RouteExistingFiles = false;

			routes.MapRoute("api", "api/{method}", new { controller = "ReportingApi", action="Dispatch", service = this });
		
			//TODO default route for error
		}

		#endregion

		#region IReportingService implementation

		public void Dispose()
		{
			//TODO
		}

		public bool Ping()
		{
			return true;
		}

		public void RunReport(Guid taskId)
		{
			throw new NotImplementedException();
		}

		public bool CancelReport(Guid taskId)
		{
			throw new NotImplementedException();
		}

		public bool IsRunning(Guid taskId)
		{
			throw new NotImplementedException();
		}

		public bool IsWaitingForRun(Guid taskid)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public static class Initializer
	{
		public static void Initialize()
		{
			new ReportingService().Initialize();
		}
	}
}
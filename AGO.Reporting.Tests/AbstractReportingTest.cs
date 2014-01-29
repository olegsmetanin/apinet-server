using AGO.Core.Application;
using AGO.Core.Controllers;
using AGO.Core.Model.Reporting;
using AGO.Core.Model.Security;
using AGO.Reporting.Common;
using NHibernate;

namespace AGO.Reporting.Tests
{
	public class AbstractReportingTest : AbstractControllersApplication
	{
		protected override void DoRegisterPersistence()
		{
			base.DoRegisterPersistence();
			IocContainer.Register<IReportingRepository, ReportingRepository>();
		}

		protected ISession Session
		{
			get { return _SessionProvider.CurrentSession; }
		}

		protected ModelHelper M { get; private set; }

		protected virtual void Init()
		{
			Initialize();
			M = new ModelHelper(() => _SessionProvider.CurrentSession, () => CurrentUser);

			LoginAdmin();

			var quick = new ReportingServiceDescriptorModel
			            	{
			            		Name = "NUnit Fast reports",
			            		EndPoint = "http://localhost:36652/api",
			            		LongRunning = false
			            	};
			var slow = new ReportingServiceDescriptorModel
			           	{
							Name = "NUnit Long-running reports",
			           		EndPoint = "http://localhost:36653/api",
			           		LongRunning = true
			           	};
			_CrudDao.Store(quick);
			_CrudDao.Store(slow);
			_SessionProvider.CloseCurrentSession();
		}

		protected virtual void Cleanup()
		{
			var conn = _SessionProvider.CurrentSession.Connection;
			ExecuteNonQuery(@"delete from ""Core"".""ReportingServiceDescriptorModel"" where ""Name"" like 'NUnit%'", conn);
			Logout();
		}

		protected virtual void TearDown()
		{
			var conn = _SessionProvider.CurrentSession.Connection;
			ExecuteNonQuery(@"
					delete from ""Core"".""ReportTaskModel"" where ""Name"" like 'NUnit%'
					go
					delete from ""Core"".""ReportSettingModel"" where ""Name"" like 'NUnit%'
					go
					delete from ""Core"".""ReportTemplateModel"" where ""Name"" like 'NUnit%'
					go
					truncate ""Core"".""WorkQueue""", conn);
			_SessionProvider.CloseCurrentSession();
		}

		protected UserModel Login(string email, string pwd)
		{
			return IocContainer.GetInstance<AuthController>().Login(email, pwd) as UserModel;
		}

		protected UserModel LoginAdmin()
		{
			return Login("admin@apinet-test.com", "1");
		}

		protected void Logout()
		{
			IocContainer.GetInstance<AuthController>().Logout();
		}

		protected UserModel CurrentUser
		{
			get { return IocContainer.GetInstance<AuthController>().CurrentUser(); }
		}
	}
}
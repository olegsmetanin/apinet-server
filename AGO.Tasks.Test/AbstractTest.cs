using System;
using AGO.Core.Application;
using AGO.Core.Controllers;
using AGO.Core.Model.Security;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Projects;
using NHibernate;

namespace AGO.Tasks.Test
{
	public class AbstractTest : AbstractControllersApplication
	{
		protected string TestProject { get; private set; }

		protected ISession Session
		{
			get { return _SessionProvider.CurrentSession; }
		}

		protected ModelHelper M { get; private set; }

		protected virtual void Init()
		{
			Initialize();
			TestProject = Guid.NewGuid().ToString().Replace("-", string.Empty);
			M = new ModelHelper(() => _SessionProvider.CurrentSession, TestProject, () => CurrentUser);

			var admin = LoginAdmin();
			
			var type = new ProjectTypeModel
			{
			    Creator = admin,
				ProjectCode = TestProject,
			    Name = "Управление задачами",
			    Module = typeof (ModuleDescriptor).Assembly.FullName
			};
			_CrudDao.Store(type);

			var p = new ProjectModel
			{
				Creator = admin,
			    ProjectCode = TestProject,
			    Name = "Unit test project: " + TestProject,
				Type = type,
				Status = ProjectStatus.Doing
			};
			_CrudDao.Store(p);

			var projAdmin = new ProjectParticipantModel
			{
			    Project = p,
			    User = admin,
			    GroupName = "Managers",
			    IsDefaultGroup = true
			};
			_CrudDao.Store(projAdmin);

			_SessionProvider.CloseCurrentSession();
		}

		protected virtual void Cleanup()
		{
			var conn = _SessionProvider.CurrentSession.Connection;
			ExecuteNonQuery(string.Format(@"
					delete from ""Core"".""ProjectStatusHistoryModel"" where
						""ProjectId"" in (select ""Id"" from ""Core"".""ProjectModel"" where ""ProjectCode"" = '{0}')
					go
					delete from ""Core"".""ProjectParticipantModel"" where
						""ProjectId"" in (select ""Id"" from ""Core"".""ProjectModel"" where ""ProjectCode"" = '{0}')
					go
					delete from ""Core"".""ProjectModel"" where ""ProjectCode"" = '{0}'
					go
					delete from ""Core"".""ProjectTypeModel"" where ""ProjectCode"" = '{0}'", 
					TestProject), conn);
			_SessionProvider.CloseCurrentSession();
			Logout();
		}

		protected virtual void TearDown()
		{
			var conn = _SessionProvider.CurrentSession.Connection;
			ExecuteNonQuery(string.Format(@"
					delete from ""Tasks"".""TaskStatusHistoryModel"" where 
						""TaskId"" in (select ""Id"" from ""Tasks"".""TaskModel"" where ""ProjectCode"" = '{0}')
					go
					delete from ""Tasks"".""TaskExecutorModel"" where
						""TaskId"" in (select ""Id"" from ""Tasks"".""TaskModel"" where ""ProjectCode"" = '{0}')
					go
					delete from ""Tasks"".""TaskAgreementModel"" where
						""TaskId"" in (select ""Id"" from ""Tasks"".""TaskModel"" where ""ProjectCode"" = '{0}')
					go
					delete from ""Tasks"".""TaskToTagModel"" where
						""TaskId"" in (select ""Id"" from ""Tasks"".""TaskModel"" where ""ProjectCode"" = '{0}')
					go
					delete from ""Core"".""TagModel"" where ""ProjectCode"" = '{0}'
					go
					delete from ""Core"".""CustomPropertyInstanceModel"" where
						""TaskId"" in (select ""Id"" from ""Tasks"".""TaskModel"" where ""ProjectCode"" = '{0}')
					go
					delete from ""Core"".""CustomPropertyTypeModel"" where ""ProjectCode"" = '{0}'
					go
					delete from ""Tasks"".""TaskModel"" where ""ProjectCode"" = '{0}'
					go
					delete from ""Tasks"".""TaskTypeModel"" where ""ProjectCode"" = '{0}'", 
					TestProject), conn);
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
using System;
using AGO.Core.Application;
using AGO.Core.Controllers;
using AGO.Core.Model.Security;
using AGO.Home.Model.Dictionary.Projects;
using AGO.Home.Model.Projects;
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
			var status = new ProjectStatusModel
			             	{
			             		Creator = admin,
			             		ProjectCode = TestProject,
			             		Name = "В работе",
			             	};
			_CrudDao.Store(status);

			var p = new ProjectModel
			        	{
							Creator = admin,
			        		ProjectCode = TestProject,
			        		Name = "Unit test project: " + TestProject,
							Type = type,
							Status = status
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
					delete from Home.ProjectStatusHistoryModel where
						ProjectId in (select Id from Home.ProjectModel where ProjectCode = '{0}')
					go
					delete from Home.ProjectParticipantModel where
						ProjectId in (select Id from Home.ProjectModel where ProjectCode = '{0}')
					go
					delete from Home.ProjectModel where ProjectCode = '{0}'
					go
					delete from Home.ProjectStatusModel where ProjectCode = '{0}'
					go
					delete from Home.ProjectTypeModel where ProjectCode = '{0}'
					go", TestProject), conn);
			_SessionProvider.CloseCurrentSession();
			Logout();
		}

		protected virtual void TearDown()
		{
			var conn = _SessionProvider.CurrentSession.Connection;
			ExecuteNonQuery(string.Format(@"
					delete from Tasks.CustomTaskStatusHistoryModel where 
						TaskId in (select Id from Tasks.TaskModel where ProjectCode = '{0}')
					go
					delete from Tasks.TaskStatusHistoryModel where 
						TaskId in (select Id from Tasks.TaskModel where ProjectCode = '{0}')
					go
					delete from Tasks.TaskExecutorModel where
						TaskId in (select Id from Tasks.TaskModel where ProjectCode = '{0}')
					go
					delete from Tasks.TaskAgreementModel where
						TaskId in (select Id from Tasks.TaskModel where ProjectCode = '{0}')
					go
					delete from Core.CustomPropertyInstanceModel where
						TaskId in (select Id from Tasks.TaskModel where ProjectCode = '{0}')
					go
					delete from Core.CustomPropertyTypeModel where ProjectCode = '{0}'
					go
					delete from Tasks.TaskModel where ProjectCode = '{0}'
					go
					delete from Tasks.TaskTypeModel where ProjectCode = '{0}'
					go
					delete from Tasks.CustomTaskStatusModel where ProjectCode = '{0}'
					go", TestProject), conn);
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
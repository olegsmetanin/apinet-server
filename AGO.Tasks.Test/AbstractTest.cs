using System;
using AGO.Core.Application;
using AGO.Core.Model.Security;
using AGO.Home.Model.Dictionary.Projects;
using AGO.Home.Model.Projects;
using NHibernate;

namespace AGO.Tasks.Test
{
	public class AbstractTest : AbstractTestFixture
	{
		protected string TestProject { get; private set; }

		protected ISession Session
		{
			get { return _SessionProvider.CurrentSession; }
		}

		protected virtual void Init()
		{
			InitContainer();
			TestProject = Guid.NewGuid().ToString().Replace("-", string.Empty);

			var admin = Session.QueryOver<UserModel>().Where(m => m.Login == "admin@agosystems.com").SingleOrDefault();
			var type = new ProjectTypeModel
			           	{
			           		Creator = admin,
							ProjectCode = TestProject,
			           		Name = "Управления задачами",
			           		Module = typeof (ModuleDescriptor).Assembly.FullName
			           	};
			Session.Save(type);
			var status = new ProjectStatusModel
			             	{
			             		Creator = admin,
			             		ProjectCode = TestProject,
			             		Name = "В работе",
			             	};
			Session.Save(status);

			var p = new ProjectModel
			        	{
							Creator = admin,
			        		ProjectCode = TestProject,
			        		Name = "Unit test project: " + TestProject,
							Type = type,
							Status = status
			        	};
			Session.Save(p);

			var participant = new ProjectParticipantModel
			                  	{
			                  		Project = p,
			                  		User = admin,
			                  		GroupName = "Managers",
			                  		IsDefaultGroup = true
			                  	};
			Session.Save(participant);
			_SessionProvider.CloseCurrentSession();
		}

		protected virtual void Cleanup()
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
					delete from Tasks.TaskModel where ProjectCode = '{0}'
					go
					delete from Tasks.TaskTypeModel where ProjectCode = '{0}'
					go
					delete from Tasks.CustomTaskStatusModel where ProjectCode = '{0}'
					go
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
		}
	}
}
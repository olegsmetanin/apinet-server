using System;
using AGO.Core.Application;
using AGO.Tasks.Controllers;
using NHibernate;

namespace AGO.Tasks.Test
{
	public class AbstractDictionaryTest: AbstractTestFixture
	{
		protected string TestProject { get; private set; }
		protected DictionaryController Controller { get; private set; }

		protected virtual void Init()
		{
			Initialize();
			TestProject = Guid.NewGuid().ToString().Replace("-", string.Empty);
			Controller = IocContainer.GetInstance<DictionaryController>();
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
					delete from Tasks.TaskModel where ProjectCode = '{0}'
					go
					delete from Tasks.TaskTypeModel where ProjectCode = '{0}'
					go
					delete from Tasks.CustomTaskStatusModel where ProjectCode = '{0}'
					go", TestProject), conn);
			_SessionProvider.CloseCurrentSession();
		}

		protected ISession Session
		{
			get { return _SessionProvider.CurrentSession; }
		}
	}
}
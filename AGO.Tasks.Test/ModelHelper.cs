using System;
using AGO.Core.Model.Security;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using NHibernate;

namespace AGO.Tasks.Test
{
	public class ModelHelper
	{
		private readonly Func<ISession> session;
		private readonly string project;
		private readonly Func<UserModel> currentUser;

		public ModelHelper(Func<ISession> session, string project, Func<UserModel> currentUser)
		{
			this.session = session;
			this.project = project;
			this.currentUser = currentUser;
		}

		public TaskModel Task(int num, TaskTypeModel type, string content = null, TaskStatus status = TaskStatus.NotStarted)
		{
			var task = new TaskModel
			{
				Creator = currentUser(),
				ProjectCode = project,
				InternalSeqNumber = num,
				SeqNumber = "t0-" + num,
				TaskType = type,
				Content = content,
				Status = status
			};
			session().Save(task);

			return task;
		}

		public TaskModel Task(int num, string type = "testType", string content = null, TaskStatus status = TaskStatus.NotStarted)
		{
			var typeModel = session().QueryOver<TaskTypeModel>()
				.Where(m => m.ProjectCode == project && m.Name == type).SingleOrDefault() ?? TaskType(type);

			return Task(num, typeModel, content, status);
		}

		public TaskTypeModel TaskType(string name = "TestTaskType")
		{
			var m = new TaskTypeModel
			       	{
						Creator = currentUser(),
			       		ProjectCode = project, 
						Name = name
			       	};
			session().Save(m);

			return m;
		}

		public CustomTaskStatusModel CustomStatus(string name = "status", byte order = 0)
		{
			var m = new CustomTaskStatusModel
			        	{
							Creator = currentUser(),
			        		ProjectCode = project,
			        		Name = name,
							ViewOrder = order
			        	};
			session().Save(m);

			return m;
		}
	}
}
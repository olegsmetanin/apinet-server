using System;
using AGO.Core.Model.Dictionary;
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
				Content = content
			};
			task.ChangeStatus(status, currentUser());
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

		public TaskCustomPropertyModel Param(TaskModel task, string name, object value)
		{
			var pvt = value is DateTime
			          	? CustomPropertyValueType.Date
			          	: value is decimal
			          	  	? CustomPropertyValueType.Number
			          	  	: CustomPropertyValueType.String;
			var paramType = session().QueryOver<CustomPropertyTypeModel>()
				.Where(m => m.ProjectCode == project && m.FullName == name).SingleOrDefault() ?? ParamType(name, pvt);

			return Param(task, paramType, value);
		}

		public TaskCustomPropertyModel Param(TaskModel task, CustomPropertyTypeModel paramType, object value)
		{
			var p = new TaskCustomPropertyModel
			        	{
			        		Task = task,
			        		Creator = currentUser(),
			        		PropertyType = paramType,
			        		Value = value
			        	};
			session().Save(p);

			return p;
		}

		public CustomPropertyTypeModel ParamType(string name = "strParam", CustomPropertyValueType type = CustomPropertyValueType.String)
		{
			var pt = new CustomPropertyTypeModel
			         	{
			         		ProjectCode = project,
			         		Creator = currentUser(),
			         		Name = name,
							FullName = name,
			         		ValueType = type
			         	};
			session().Save(pt);

			return pt;
		}
	}
}
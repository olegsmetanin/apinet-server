using System;
using System.Linq;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Security;
using AGO.Home.Model.Projects;
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

		public TaskModel Task(int num, TaskTypeModel type, string content = null, TaskStatus status = TaskStatus.New)
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
			ProjectParticipantModel ppm = null;
			ProjectModel pm = null;
			var participant = session().QueryOver(() => ppm)
				.JoinAlias(() => ppm.Project, () => pm)
				.Where(() => pm.ProjectCode == project)
				.List<ProjectParticipantModel>().FirstOrDefault();
			var executor = new TaskExecutorModel {Creator = currentUser(), Task = task, Executor = participant};
			task.Executors.Add(executor);
			
			session().Save(task);
			session().FlushMode = FlushMode.Auto;

			return task;
		}

		public TaskModel Task(int num, string type = "testType", string content = null, TaskStatus status = TaskStatus.New)
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
			session().FlushMode = FlushMode.Auto;

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
			session().FlushMode = FlushMode.Auto;

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
			session().FlushMode = FlushMode.Auto;

			return pt;
		}

		public TaskTagModel Tag(string name = "testTag", TaskTagModel parent = null, UserModel owner = null)
		{
			var tag = new TaskTagModel
			          	{
			          		ProjectCode = project,
			          		Creator = currentUser(),
			          		Name = name,
							FullName = parent != null ? parent.FullName + " \\ " + name : name,
			          		Parent = parent,
			          		Owner = owner ?? currentUser()
			          	};
			session().Save(tag);
			session().FlushMode = FlushMode.Auto;

			return tag;
		}
	}
}
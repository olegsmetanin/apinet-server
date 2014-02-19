using System;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Security;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using NHibernate;

namespace AGO.Tasks.Test
{
	public class ModelHelper: Core.Tests.ModelHelper
	{
		private readonly string project;

		public ModelHelper(Func<ISession> session, Func<UserModel> currentUser, string project)
			:base(session, currentUser)
		{
			this.project = project;
		}

		public TaskModel Task(int num, TaskTypeModel type, string content = null, TaskStatus status = TaskStatus.New)
		{
			return Track(() =>
			{
				var task = new TaskModel
				{
					Creator = CurrentUser(),
					ProjectCode = project,
					InternalSeqNumber = num,
					SeqNumber = "t0-" + num,
					TaskType = type,
					Content = content
				};
				task.ChangeStatus(status, CurrentUser());
				var executor = new TaskExecutorModel
				{
					Creator = CurrentUser(),
					Task = task,
					Executor = MemberFromUser(project)
				};
				task.Executors.Add(executor);

				Session().Save(task);
				Session().Flush();

				return task;
			});
		}

		public TaskModel Task(int num, string type = "testType", string content = null, TaskStatus status = TaskStatus.New)
		{
			var typeModel = Session().QueryOver<TaskTypeModel>()
				.Where(m => m.ProjectCode == project && m.Name == type).SingleOrDefault() ?? TaskType(type);

			return Task(num, typeModel, content, status);
		}

		public TaskTypeModel TaskType(string name = "TestTaskType")
		{
			return Track(() =>
			{
				var m = new TaskTypeModel
				{
					Creator = CurrentUser(),
					ProjectCode = project,
					Name = name
				};

				Session().Save(m);
				Session().Flush();

				return m;
			});
		}

		public TaskCustomPropertyModel Param(TaskModel task, string name, object value)
		{
			var pvt = value is DateTime
			          	? CustomPropertyValueType.Date
			          	: value is decimal
			          	  	? CustomPropertyValueType.Number
			          	  	: CustomPropertyValueType.String;
			var paramType = Session().QueryOver<CustomPropertyTypeModel>()
				.Where(m => m.ProjectCode == project && m.FullName == name).SingleOrDefault() ?? ParamType(name, pvt);

			return Param(task, paramType, value);
		}

		public TaskCustomPropertyModel Param(TaskModel task, CustomPropertyTypeModel paramType, object value)
		{
			return Track(() =>
			{
				var p = new TaskCustomPropertyModel
				{
					Task = task,
					Creator = CurrentUser(),
					PropertyType = paramType,
					Value = value
				};
				Session().Save(p);
				Session().Flush();

				return p;
			});
		}

		public CustomPropertyTypeModel ParamType(string name = "strParam", CustomPropertyValueType type = CustomPropertyValueType.String)
		{
			return Track(() =>
			{
				var pt = new CustomPropertyTypeModel
				{
					ProjectCode = project,
					Creator = CurrentUser(),
					Name = name,
					FullName = name,
					ValueType = type
				};
				Session().Save(pt);
				Session().Flush();

				return pt;
			});
		}

		public TaskTagModel Tag(string name = "testTag", TaskTagModel parent = null, UserModel owner = null)
		{
			return Track(() =>
			{
				var tag = new TaskTagModel
				{
					ProjectCode = project,
					Creator = owner ?? CurrentUser(),
					Name = name,
					FullName = parent != null ? parent.FullName + " \\ " + name : name,
					Parent = parent
				};
				if (parent != null) parent.Children.Add(tag);
				Session().Save(tag);
				Session().Flush();

				return tag;
			});
		}
	}
}
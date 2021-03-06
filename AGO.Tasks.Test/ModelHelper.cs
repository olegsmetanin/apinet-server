﻿using System;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using NHibernate;
using TaskStatus = AGO.Tasks.Model.Task.TaskStatus;

namespace AGO.Tasks.Test
{
	public class ModelHelper: Core.Tests.ModelHelper
	{
		private readonly string project;
		private readonly Func<ISession> projSession;

		public ModelHelper(Func<ISession> session, Func<ISession> projSession, Func<UserModel> currentUser, string project)
			:base(session, currentUser)
		{
			this.project = project;
			this.projSession = projSession;
		}

		protected override ISession ProjectSession(string projCode)
		{
			return projSession();
		}

		protected override void InternalDelete(object model)
		{
			var p = model as ProjectModel;
			if (p != null)
			{
				DeleteProjectMembers(p.ProjectCode, projSession());
				DeleteProjectActivity(p.ProjectCode, projSession());
			}
			base.InternalDelete(model);
		}

		public TaskModel Task(int num, TaskTypeModel type, string content = null, TaskStatus status = TaskStatus.New, UserModel creator = null, UserModel executor = null)
		{
			return Track(() =>
			{
				var task = new TaskModel
				{
					Creator = MemberFromUser(project, creator ?? CurrentUser()),
					ProjectCode = project,
					InternalSeqNumber = num,
					SeqNumber = "t0-" + num,
					TaskType = type,
					Content = content
				};
				task.ChangeStatus(status, MemberFromUser(project, creator ?? CurrentUser()));
				var e = new TaskExecutorModel
				{
					Creator = MemberFromUser(project, creator ?? CurrentUser()),
					Task = task,
					Executor = MemberFromUser(project, executor)
				};
				task.Executors.Add(e);

				Session().Save(task);
				Session().Flush();

				return task;
			});
		}

		public TaskModel Task(int num, string type = "testType", string content = null, TaskStatus status = TaskStatus.New, UserModel creator = null, UserModel executor = null)
		{
			var typeModel = Session().QueryOver<TaskTypeModel>()
				.Where(m => m.ProjectCode == project && m.Name == type).SingleOrDefault() ?? TaskType(type);

			return Task(num, typeModel, content, status, creator, executor);
		}

		public TaskAgreementModel Agreement(TaskModel task, UserModel agreemer = null, UserModel creator = null, bool done = false)
		{
			var agr = new TaskAgreementModel
			{
				Creator = MemberFromUser(task.ProjectCode, creator ?? agreemer ?? CurrentUser()),
				Task = task,
				Agreemer = MemberFromUser(project, agreemer ?? CurrentUser()),
				Done = done,
				AgreedAt = done ? DateTime.UtcNow : (DateTime?) null
			};
			task.Agreements.Add(agr);
			Session().Update(task);
			Session().Flush();
			return agr;
		}

		public TaskTypeModel TaskType(string name = "TestTaskType", UserModel creator = null)
		{
			return Track(() =>
			{
				var m = new TaskTypeModel
				{
					Creator = MemberFromUser(project, ( creator ?? CurrentUser())),
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
					Creator = MemberFromUser(task.ProjectCode, CurrentUser()),
					PropertyType = paramType,
					Value = value
				};
				task.CustomProperties.Add(p);
				Session().Save(p);
				Session().Flush();

				return p;
			});
		}

		public CustomPropertyTypeModel ParamType(string name = "strParam", CustomPropertyValueType type = CustomPropertyValueType.String, CustomPropertyTypeModel parent = null)
		{
			return Track(() =>
			{
				var pt = new CustomPropertyTypeModel
				{
					ProjectCode = project,
					Creator = MemberFromUser(project, CurrentUser()),
					Parent = parent,
					Name = name,
					FullName = parent != null ? parent.Name + " / " + name : name,
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
					OwnerId = (owner ?? CurrentUser()).Id,
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

		public TaskFileModel File(TaskModel task, string name = "testFile", long size = 1, string mime = "application/txt",
			UserModel creator = null)
		{
			return Track(() =>
			{
				var file = new TaskFileModel
				{
					Owner = task,
					Name = name,
					ContentType = mime,
					Size = size,
					Creator = MemberFromUser(task.ProjectCode, creator ?? CurrentUser())
				};
				task.Files.Add(file);
				Session().Save(file);
				Session().Flush();

				return file;
			});
		}

		public TaskTimelogEntryModel Time(TaskModel task, UserModel executor, decimal time = 1m, string comment = null)
		{
			return Track(() =>
			{
				var entry = task.TrackTime(executor, time, comment);
				Session().Save(entry);
				Session().Flush();
				return entry;
			});
		}

	    public TaskCommentModel Comment(TaskModel task, string text = "task comment", UserModel author = null)
	    {
	        return Track(() =>
	        {
	            var comment = new TaskCommentModel
	            {
	                Task = task,
	                Text = text,
	                Creator = MemberFromUser(task.ProjectCode, author ?? CurrentUser()),
	                CreationTime = DateTime.UtcNow
	            };
	            task.Comments.Add(comment);
	            Session().Save(comment);
	            Session().Flush();
	            return comment;
	        });
	    }
	}
}
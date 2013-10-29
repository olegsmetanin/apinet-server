using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Security;
using AGO.Home.Model.Dictionary.Projects;
using AGO.Home.Model.Projects;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using NHibernate;

namespace AGO.Tasks
{
	public class ModuleTestDataService : AbstractService, IModuleTestDataService
	{
		#region Properties, fields, constructors

		protected ISessionProvider _SessionProvider;

		protected ICrudDao _CrudDao;

		protected ISession CurrentSession { get { return _SessionProvider.CurrentSession; } }

		public ModuleTestDataService(
			ISessionProvider sessionProvider,
			ICrudDao crudDao)
		{
			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");
			_SessionProvider = sessionProvider;

			if (crudDao == null)
				throw new ArgumentNullException("crudDao");
			_CrudDao = crudDao;
		}

		#endregion

		#region Interfaces implementation

		public void Populate()
		{
			var admin = CurrentSession.QueryOver<UserModel>()
			    .Where(m => m.SystemRole == SystemRole.Administrator).Take(1).List().FirstOrDefault();
			var context = new
			{
				Admin = admin,
				User1 = CurrentSession.QueryOver<UserModel>()
					.Where(m => m.Login == "user1@apinet-test.com").Take(1).List().FirstOrDefault(),
				User2 = CurrentSession.QueryOver<UserModel>()
					.Where(m => m.Login == "user2@apinet-test.com").Take(1).List().FirstOrDefault(),
				User3 = CurrentSession.QueryOver<UserModel>()
					.Where(m => m.Login == "user3@apinet-test.com").Take(1).List().FirstOrDefault(),
				InitialProjectStatus = CurrentSession.QueryOver<ProjectStatusModel>()
					.Where(m => m.IsInitial).Take(1).List().FirstOrDefault(),
				CommonTag = CurrentSession.QueryOver<ProjectTagModel>()
					.Where(m => m.Name == "Common tag").Take(1).List().FirstOrDefault(),
				ProjectType = new ProjectTypeModel
				{
					Creator = admin,
					Name = "Task management project",
					Module = GetType().Assembly.FullName
				}
			};

			if (context.Admin == null || context.User1 == null || context.User2 == null || context.User3 == null ||
					context.InitialProjectStatus == null || context.CommonTag == null)
				throw new Exception("Test data inconsistency");

			_CrudDao.Store(context.ProjectType);

			foreach (var project in DoPopulateProjects(context))
			{
				DoPopulateDepartments(context, project);

				DoPopulateCustomStatuses(context, project);

				DoPopulateTasks(context, project,
					DoPopulateTaskTypes(context, project),
					DoPopulatePropertyTypes(context, project));
			}			
		}

		#endregion

		#region Helper methods

		protected IEnumerable<ProjectModel> DoPopulateProjects(dynamic context)
		{
			Func<int, ProjectModel> createProject = index =>
			{
				var project = new ProjectModel
				{
					Creator = context.Admin,
					ProjectCode = "tasks_" + index,
					Name = "Task management project " + index,
					Description = "Description of task management project " + index,
					Type = context.ProjectType,
					Status = context.InitialProjectStatus,
				};
				_CrudDao.Store(project);

				_CrudDao.Store(new ProjectStatusHistoryModel
				{
					Creator = context.Admin,
					StartDate = DateTime.Now,
					Project = project,
					Status = context.InitialProjectStatus
				});

				_CrudDao.Store(new ProjectParticipantModel
				{
					User = context.Admin,
					Project = project
				});

				_CrudDao.Store(new ProjectParticipantModel
				{
					User = context.User1,
					Project = project
				});

				_CrudDao.Store(new ProjectParticipantModel
				{
					User = context.User2,
					Project = project
				});

				_CrudDao.Store(new ProjectParticipantModel
				{
					User = context.User3,
					Project = project
				});

				_CrudDao.Store(new ProjectToTagModel
				{
					Creator = context.Admin,
					Project = project,
					Tag = context.CommonTag
				});

				return project;
			};

			var result = new List<ProjectModel>();
			for (var i = 1; i < 11; i++)
				result.Add(createProject(i));

			return result;
		}

		protected dynamic DoPopulateDepartments(dynamic context, ProjectModel project)
		{
			var primaryDepartment = new DepartmentModel
			{
				ProjectCode = project.ProjectCode,
				Creator = context.Admin,
				Name = "Primary department",
				FullName = "Primary department"
			};
			_CrudDao.Store(primaryDepartment);

			context.Admin.Departments.Add(primaryDepartment);
			_CrudDao.Store(context.Admin);

			Action<string, UserModel> createDepartment = (name, user) =>
			{
				var department = new DepartmentModel
				{
					ProjectCode = project.ProjectCode,
					Creator = context.Admin,
					Name = name,
					FullName = string.Format("{0} / {1}", primaryDepartment.Name, name),
					Parent = primaryDepartment
				};
				_CrudDao.Store(department);
				user.Departments.Add(department);
				_CrudDao.Store(user);
			};

			createDepartment("Child department 1", context.User1);
			createDepartment("Child department 2", context.User2);
			createDepartment("Child department 3", context.User3);

			return context;
		}

		protected void DoPopulateCustomStatuses(dynamic context, ProjectModel project)
		{
			byte order = 0;
			Action<string> createCustomStatus = name =>
			{
				var customStatus = new CustomTaskStatusModel
				{
					Creator = context.Admin,
					ProjectCode = project.ProjectCode,
					Name = name,
					ViewOrder = order++
				};
				_CrudDao.Store(customStatus);
			};

			createCustomStatus("Preparing");
			createCustomStatus("Sent to work");
			createCustomStatus("In progress");
			createCustomStatus("Complete");
			createCustomStatus("Closed");
			createCustomStatus("Suspended");
		}

		protected dynamic DoPopulateTaskTypes(dynamic context, ProjectModel project)
		{
			Func<string, TaskTypeModel> factory = name =>
			{
				var taskType = new TaskTypeModel
				{
					Creator = context.Admin,
					ProjectCode = project.ProjectCode,
					Name = name
				};
				_CrudDao.Store(taskType);
				return taskType;
			};

			return new
			{
				Inventory = factory("Inventory"),
				Measurement = factory("Measurement"),
				Calculations = factory("Calculations"),
				PreparePaymentDocs = factory("Prepare payment documents"),
				CleanArchives = factory("Clean archives"),
				PrepareWorkPlace = factory("Prepare work place")
			};
		}

		protected dynamic DoPopulatePropertyTypes(dynamic context, ProjectModel project)
		{
			Func<string, CustomPropertyValueType, CustomPropertyTypeModel> factory = (name, type) =>
			{
				var propertyType = new CustomPropertyTypeModel
				{
					ProjectCode = project.ProjectCode,
					Creator = context.Admin,
					Name = name,
					FullName = name,
					ValueType = type
				};
				_CrudDao.Store(propertyType);
				return propertyType;
			};

			return new
			{
				String1 = factory("String 1", CustomPropertyValueType.String),
				Number1 = factory("Number 1", CustomPropertyValueType.Number),
				Date1 = factory("Date 1", CustomPropertyValueType.Date)
			};
		}

		protected void DoPopulateTasks(dynamic context, ProjectModel project, dynamic taskTypes, dynamic propertyTypes)
		{
			var seqnum = 1;
			Func<TaskTypeModel, TaskStatus, TaskPriority, string, DateTime?, TaskModel> createTask =
				(type, status, priority, content, dueDate) =>
			{
				var task = new TaskModel
				{
					Creator = context.Admin,
					ProjectCode = project.ProjectCode,
					InternalSeqNumber = seqnum,
					SeqNumber = "t0-" + seqnum,
					Priority = priority,
					Content = content,
					DueDate = dueDate,
					TaskType = type
				};
				task.ChangeStatus(status, context.Admin);
				seqnum++;
				_CrudDao.Store(task);
				return task;
			};

			var t1 = createTask(taskTypes.Inventory, TaskStatus.NotStarted, TaskPriority.Normal, null, null);
			createTask(taskTypes.Calculations, TaskStatus.InWork, TaskPriority.High,
				"Calculations 2", DateTime.Now.AddDays(2));
			createTask(taskTypes.Measurement, TaskStatus.Completed, TaskPriority.Low,
				"Do measurements of object on address: MO, Korolev, Kosmonavtov st., 12, 2", DateTime.Now.AddDays(3));
			createTask(taskTypes.Inventory, TaskStatus.NotStarted, TaskPriority.High, null, DateTime.Now.AddDays(-1));

			Action<TaskModel, CustomPropertyTypeModel, object> createTaskProperty = (task, propertyType, value) =>
			{
				var taskProperty = new TaskCustomPropertyModel
				{
					Task = task,
					Creator = context.Admin,
					PropertyType = propertyType,
					Value = value
				};
				_CrudDao.Store(taskProperty);
			};

			createTaskProperty(t1, propertyTypes.String1, "some string data");
			createTaskProperty(t1, propertyTypes.Number1, 12.3);
			createTaskProperty(t1, propertyTypes.Date1, new DateTime(2013, 01, 01));
		}

		#endregion
	}
}
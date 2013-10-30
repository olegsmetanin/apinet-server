using System;
using System.Linq;
using AGO.Core;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Security;
using AGO.Home.Model.Dictionary.Projects;
using AGO.Home.Model.Projects;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
namespace AGO.Tasks
{
	public class TestDataService : AbstractTestDataService, ITestDataService
	{
		#region Properties, fields, constructors

		public TestDataService(ISessionProvider sessionProvider, ICrudDao crudDao)
			: base(sessionProvider, crudDao)
		{
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
				UrgentTag = CurrentSession.QueryOver<ProjectTagModel>()
					.Where(m => m.Name == "Urgent").SingleOrDefault(),
				AdminTags = CurrentSession.QueryOver<ProjectTagModel>()
					.Where(m => m.Owner == admin).List(),
				ProjectType = new ProjectTypeModel
				{
					Creator = admin,
					Name = "Task management project",
					Module = GetType().Assembly.FullName
				}
			};

			if (context.Admin == null || context.User1 == null || context.User2 == null ||
					context.User3 == null || context.InitialProjectStatus == null || context.UrgentTag == null)
				throw new Exception("Test data inconsistency");

			_CrudDao.Store(context.ProjectType);

			var projects = DoPopulateProjects(context);

			DoPopulateCustomStatuses(context, projects.Project1);
			DoPopulateTasks(context, projects.Project1,
				DoPopulateTaskTypes(context, projects.Project1),
				DoPopulatePropertyTypes(context, projects.Project1));	
		}

		#endregion

		#region Helper methods

		protected dynamic DoPopulateProjects(dynamic context)
		{
			Func<int, string, string, ProjectModel> createProject = (index, name, description) =>
			{
				var project = new ProjectModel
				{
					Creator = context.Admin,
					ProjectCode = "tasks_" + index,
					Name = name ?? ("Task management project " + index),
					Description = description ?? ("Description of task management project " + index),
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
					Tag = context.UrgentTag
				});

				foreach (var tag in context.AdminTags)
				{
					_CrudDao.Store(new ProjectToTagModel
					{
						Creator = context.Admin,
						Project = project,
						Tag = tag
					});
				}

				return project;
			};

			return new
			{
				Project1 = createProject(1, "Common tasks", "Common everyday tasks for specialists")
			};
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
				PossibleIssues = factory("Possible issues", CustomPropertyValueType.String),
				ApproximateArea = factory("Approximate area", CustomPropertyValueType.Number),
				BuildingDate = factory("Building date", CustomPropertyValueType.Date),
				LastOverhaulDate = factory("Last overhaul date", CustomPropertyValueType.Date)
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

			var t1 = createTask(taskTypes.Measurement, TaskStatus.Completed, TaskPriority.Low,
				"Do measurements of object on address: MO, Korolev, Kosmonavtov st., 12, 2", DateTime.Now.AddDays(3));
			
			createTask(taskTypes.Inventory, TaskStatus.NotStarted, TaskPriority.Normal, null, null);
			createTask(taskTypes.Calculations, TaskStatus.InWork, TaskPriority.High, "Calculate year-ending salary bonuses", DateTime.Now.AddDays(2));
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

			createTaskProperty(t1, propertyTypes.PossibleIssues, "Entrance only through basement");
			createTaskProperty(t1, propertyTypes.ApproximateArea, 86.5);
			createTaskProperty(t1, propertyTypes.BuildingDate, new DateTime(1967, 04, 16));
			createTaskProperty(t1, propertyTypes.LastOverhaulDate, new DateTime(1990, 07, 01));
		}

		#endregion
	}
}
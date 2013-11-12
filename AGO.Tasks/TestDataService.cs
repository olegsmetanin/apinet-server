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

			//var statuses = DoPopulateCustomStatuses(context, projects);
			var types = DoPopulateTaskTypes(context, projects);
			var paramTypes = DoPopulatePropertyTypes(context, projects);
			DoPopulateTasks(context, projects, types, paramTypes);
		}

		#endregion

		#region Helper methods

		protected dynamic DoPopulateProjects(dynamic context)
		{
			Func<string, string, string, ProjectModel> createProject = (code, name, description) =>
			{
				var project = new ProjectModel
				{
					Creator = context.Admin,
					ProjectCode = code ?? "tasks",
					Name = name ?? "Task management project",
					Description = description ?? "Description of task management project ",
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

				var pcAdmin = new ProjectParticipantModel { User = context.Admin, Project = project };
				_CrudDao.Store(pcAdmin);
				project.Participants.Add(pcAdmin);

				var pcUser1 = new ProjectParticipantModel { User = context.User1, Project = project };
				_CrudDao.Store(pcUser1);
				project.Participants.Add(pcUser1);

				var pcUser2 = new ProjectParticipantModel { User = context.User2, Project = project };
				_CrudDao.Store(pcUser2);
				project.Participants.Add(pcUser2);

				var pcUser3 = new ProjectParticipantModel { User = context.User3, Project = project };
				_CrudDao.Store(pcUser3);
				project.Participants.Add(pcUser3);

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

				_CrudDao.Store(project);
				return project;
			};

			return new
			{
				Software = createProject("soft", "Software development project", "This is a flexible project management web application written using angularjs framework."),
				CRM = createProject("crm", "CRM tasks", "Customer relation management tasks"),
				Personal = createProject("personal", "Personal tasks", "Personal tasks project"),
				Helpdesk = createProject("hd", "Helpdesk", "Helpdesc department tasks")
			};
		}

		protected dynamic DoPopulateCustomStatuses(dynamic context, dynamic projects)
		{
			byte order = 0;
			Func<string, ProjectModel, CustomTaskStatusModel> createCustomStatus = (name, proj) =>
			{
				var customStatus = new CustomTaskStatusModel
				{
					Creator = context.Admin,
					ProjectCode = proj.ProjectCode,
					Name = name,
					ViewOrder = order++
				};
				_CrudDao.Store(customStatus);
				return customStatus;
			};

			return new
					{
						Software = new
						{
							New = createCustomStatus("New", projects.Software),
							NeedsFeedback = createCustomStatus("Needs feedback", projects.Software),
							Confirmed = createCustomStatus("Confirmed", projects.Software),
							InProgress = createCustomStatus("In progress", projects.Software),
							Resolved = createCustomStatus("Resolved", projects.Software),
							Closed = createCustomStatus("Closed", projects.Software),
							Reopened = createCustomStatus("Reopened", projects.Software)
						},
						CRM = new
						{
							Preparing = createCustomStatus("Preparing", projects.CRM),
							SentToWork = createCustomStatus("Sent to work", projects.CRM),
							InProgrecc = createCustomStatus("In progress", projects.CRM),
							Complete = createCustomStatus("Complete", projects.CRM),
							Closed = createCustomStatus("Closed", projects.CRM),
							Suspended = createCustomStatus("Suspended", projects.CRM)
						},
						Personal = new
						{
							New = createCustomStatus("New", projects.Personal),
							Open = createCustomStatus("Open", projects.Personal),
							Complete = createCustomStatus("Complete", projects.Personal)
						},
						Helpdesk = new
						{
							New = createCustomStatus("New", projects.Helpdesk),
							Confirmed = createCustomStatus("Confirmed", projects.Helpdesk),
							InProgress = createCustomStatus("Open", projects.Helpdesk),
							Resolved = createCustomStatus("Resolved", projects.Helpdesk),
							Feedback = createCustomStatus("Feedback", projects.Helpdesk),
							Closed = createCustomStatus("Closed", projects.Helpdesk),
							Reopened = createCustomStatus("Reopened", projects.Helpdesk),
							Rejected = createCustomStatus("Rejected", projects.Helpdesk)
						}
					};
		}

		protected dynamic DoPopulateTaskTypes(dynamic context, dynamic projects)
		{
			Func<string, ProjectModel, TaskTypeModel> factory = (name, proj) =>
			{
				var taskType = new TaskTypeModel
				{
					Creator = context.Admin,
					ProjectCode = proj.ProjectCode,
					Name = name
				};
				_CrudDao.Store(taskType);
				return taskType;
			};

			return new
			       	{
			       		Software = new
			       		{
							Bug = factory("Bug", projects.Software),
							Feature = factory("Feature", projects.Software),
							Issue = factory("Issue", projects.Software),
							Support = factory("Support", projects.Software)
			       		},
						CRM = new
						{
							Upselling = factory("Upselling", projects.CRM),
							Audit = factory("Audit", projects.CRM),
							Personal = factory("Personal", projects.CRM)
						},
						Personal = new
						{
						    Home = factory("Home", projects.Personal),
							Bills = factory("Bills", projects.Personal),
							Web = factory("Web", projects.Personal),
							Work = factory("Work", projects.Personal)
						},
						Helpdesk = new
						{
						    Consult = factory("Consult", projects.Helpdesk),
							Management = factory("Management", projects.Helpdesk),
							Test = factory("Test", projects.Helpdesk),
							Support = factory("Support", projects.Helpdesk)
						}
			       	};
		}

		protected dynamic DoPopulatePropertyTypes(dynamic context, dynamic projects)
		{
			Func<string, CustomPropertyValueType, ProjectModel, CustomPropertyTypeModel> factory = (name, type, proj) =>
			{
				var propertyType = new CustomPropertyTypeModel
				{
					ProjectCode = proj.ProjectCode,
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
				Software = new 
				{
					PO = factory("Issue owner", CustomPropertyValueType.String, projects.Software),
					MgrEstimate = factory("Manager estimate (hours)", CustomPropertyValueType.Number, projects.Software),
					DevEstimate = factory("Developer estimate (hours)", CustomPropertyValueType.Number, projects.Software),
					LPS = factory("Last possible start", CustomPropertyValueType.Date, projects.Software)
				},
				CRM = new
				{
					RelationsLevel = factory("Relations level", CustomPropertyValueType.String, projects.CRM),
					LastContact = factory("Last contact date", CustomPropertyValueType.Date, projects.CRM),
					BirthDay = factory("Birthday", CustomPropertyValueType.Date, projects.CRM),
					Prospects = factory("Prospects", CustomPropertyValueType.Number, projects.CRM),
				},
				Personal = new
				{
					Note = factory("Note", CustomPropertyValueType.String, projects.Personal),
					Deadline = factory("Deadline", CustomPropertyValueType.Date, projects.Personal),
				},
				Helpdesk = new
				{
					ClientSatisfaction = factory("Client satisfaction", CustomPropertyValueType.String, projects.Helpdesk),
					ClientAdequacy = factory("Client adequacy", CustomPropertyValueType.String, projects.Helpdesk),
					SpentHours = factory("Spent hours", CustomPropertyValueType.Number, projects.Helpdesk)
				}
			};
		}

		protected void DoPopulateTasks(dynamic context, dynamic projects, dynamic taskTypes, dynamic propertyTypes)
		{
			var seqnum = 0;
			Func<int, TaskTypeModel, TaskStatus, TaskPriority, string, DateTime?, ProjectModel, dynamic[], TaskModel> createTask =
				(num, type, status, priority, content, dueDate, proj, users) =>
			{
				var task = new TaskModel
				{
					Creator = context.Admin,
					ProjectCode = proj.ProjectCode,
					InternalSeqNumber = num,
					SeqNumber = "t0-" + num,
					Priority = priority,
					Content = content,
					DueDate = dueDate,
					TaskType = type
				};
				task.ChangeStatus(status, context.Admin);
				foreach (var user in users)
				{
					var executor = new TaskExecutorModel
					               	{
					               		Creator = context.Admin,
					               		Task = task,
					               		Executor = proj.Participants.First(p => p.User == user)
					               	};
					task.Executors.Add(executor);

				}
				_CrudDao.Store(task);
				return task;
			};
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

			//Software
			var sp = projects.Software;
			var stt = taskTypes.Software;
			var st1 = createTask(seqnum++, stt.Feature, TaskStatus.Completed, TaskPriority.Low,
				"Workflow configuration", DateTime.Now.AddDays(3), sp, new[] { context.User1 });
			var st2 = createTask(seqnum++, stt.Bug, TaskStatus.NotStarted, TaskPriority.High,
				"Ticket subject and text cuting when recieving from E-mail", DateTime.Now.AddDays(1), sp, new[] { context.User1 });
			var st3 = createTask(seqnum++, stt.Feature, TaskStatus.InWork, TaskPriority.Normal,
				"Encrypt emails with SMIME", DateTime.Now.AddDays(-2), sp, new[] { context.User1 });
			var st4 = createTask(seqnum++, stt.Feature, TaskStatus.Closed, TaskPriority.Normal,
				"Improve usage of label \"button_update\"", DateTime.Now.AddDays(-10), sp, new[] { context.User1 });
			var st5 = createTask(seqnum++, stt.Support, TaskStatus.Suspended, TaskPriority.Normal,
				"Plugin rollback migration", DateTime.Now.AddDays(-1), sp, new[] { context.User1 });
			var st6 = createTask(seqnum++, stt.Bug, TaskStatus.NotStarted, TaskPriority.High,
				"Can't move parent ticket between projects", DateTime.Now.AddDays(1), sp, new[] { context.User1 });

//			createTask(taskTypes.Inventory, TaskStatus.NotStarted, TaskPriority.Normal, null, null);
//			createTask(taskTypes.Calculations, TaskStatus.InWork, TaskPriority.High, "Calculate year-ending salary bonuses", DateTime.Now.AddDays(2));
//			createTask(taskTypes.Inventory, TaskStatus.NotStarted, TaskPriority.High, null, DateTime.Now.AddDays(-1));



//			createTaskProperty(t1, propertyTypes.PossibleIssues, "Entrance only through basement");
//			createTaskProperty(t1, propertyTypes.ApproximateArea, 86.5);
//			createTaskProperty(t1, propertyTypes.BuildingDate, new DateTime(1967, 04, 16));
//			createTaskProperty(t1, propertyTypes.LastOverhaulDate, new DateTime(1990, 07, 01));
		}

		#endregion
	}
}
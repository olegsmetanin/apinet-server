using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core;
using AGO.Core.DataAccess;
using AGO.Core.Model;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using Npgsql;
using TaskStatus = AGO.Tasks.Model.Task.TaskStatus;

namespace AGO.Tasks
{
	public class TestDataService : AbstractTestDataService, ITestDataService
	{
		private const string DB_SOFT = "ago_apinet_soft";
		private const string DB_OTHERS = "ago_apinet_others";

		#region Properties, fields, constructors

		public TestDataService(ISessionProviderRegistry registry, DaoFactory factory)
			: base(registry, factory)
		{
			daoCache = new Dictionary<string, ICrudDao>();
		}

		#endregion

		#region Interfaces implementation

		public IEnumerable<string> RequiredDatabases
		{
			get
			{
				yield return DB_SOFT;
				yield return DB_OTHERS;
			}
		}

		private ICrudDao mainDao;
		private readonly IDictionary<string, ICrudDao> daoCache;
		private ICrudDao ProjectDao(string project)
		{
			if (!daoCache.ContainsKey(project))
				daoCache[project] = DaoFactory.CreateProjectCrudDao(project);
			return daoCache[project];
		}

		public void Populate()
		{
			mainDao = DaoFactory.CreateMainCrudDao();
			var mainSession = SessionProviderRegistry.GetMainDbProvider().CurrentSession;

			var admin = mainDao.Find<UserModel>(q => q.Where(m => m.Email == "admin@apinet-test.com"));
			var context = new
			{
				Admin = admin,
				Demo = mainSession.QueryOver<UserModel>()
					.Where(m => m.Email == "demo@apinet-test.com").SingleOrDefault(),
				User1 = mainSession.QueryOver<UserModel>()
					.Where(m => m.Email == "user1@apinet-test.com").SingleOrDefault(),
				User2 = mainSession.QueryOver<UserModel>()
					.Where(m => m.Email == "user2@apinet-test.com").SingleOrDefault(),
				User3 = mainSession.QueryOver<UserModel>()
					.Where(m => m.Email == "user3@apinet-test.com").SingleOrDefault(),
				Artem1 = mainSession.QueryOver<UserModel>()
					.Where(m => m.Email == "artem1@twitter.com").SingleOrDefault(),
				OlegSmith = mainSession.QueryOver<UserModel>()
					.Where(m => m.Email == "olegsmith@apinet-test.com").SingleOrDefault(),
				AdminTags = mainSession.QueryOver<ProjectTagModel>()
					.Where(m => m.OwnerId == admin.Id).List(),
				ProjectType = new ProjectTypeModel
				{
					Name = "Task management project",
					Module = ModuleDescriptor.MODULE_CODE
				}
			};

			if (context.Admin == null || context.Demo == null || context.User1 == null || context.User2 == null ||
					context.User3 == null || context.Artem1 == null || context.OlegSmith == null)
				throw new Exception("Test data inconsistency");

			mainDao.Store(context.ProjectType);

			var projects = DoPopulateProjects(context);
			mainSession.Flush();

			var types = DoPopulateTaskTypes(context, projects);
			var paramTypes = DoPopulatePropertyTypes(context, projects);
			var tags = DoPopulateTags(context, projects);
			DoPopulateTasks(context, projects, types, paramTypes, tags);
			DoPopulateReports(context, projects);

			SessionProviderRegistry.CloseCurrentSessions();
		}

		#endregion

		#region Helper methods

		private ProjectMemberModel UserToMember(IProjectBoundModel project, UserModel user)
		{
			return SessionProviderRegistry.GetProjectProvider(project.ProjectCode).CurrentSession
				.QueryOver<ProjectMemberModel>()
				.Where(m => m.ProjectCode == project.ProjectCode && m.UserId == user.Id)
				.SingleOrDefault();
		}

		protected dynamic DoPopulateProjects(dynamic context)
		{
			Func<ProjectModel, UserModel, string, ProjectMemberModel> addMember = (p, u, r) =>
			{
				var member = ProjectMemberModel.FromParameters(u, p, r);
				var membership = new ProjectMembershipModel {Project = p, User = u};
				p.Members.Add(membership);
				return member;
			};

			Func<string, string, string, ProjectModel> createProject = (code, name, description) =>
			{
				var pgcsb = new NpgsqlConnectionStringBuilder(SessionProviderRegistry.GetMainDbProvider().ConnectionString)
				{
					Database = DB_SOFT.Contains(code) ? DB_SOFT : DB_OTHERS
				};
				var project = new ProjectModel
				{
					ProjectCode = code ?? "tasks",
					Name = name ?? "Task management project",
					Description = description ?? "Description of task management project ",
					Type = context.ProjectType,
					ConnectionString = pgcsb.ConnectionString
				};
				mainDao.Store(project);

				mainDao.Store(project.ChangeStatus(ProjectStatus.New, context.Admin));
				SessionProviderRegistry.GetMainDbProvider().CurrentSession.Flush();

				var pdao = ProjectDao(project.ProjectCode);
				var pmAdmin = addMember(project, context.Admin, BaseProjectRoles.Administrator);
				pmAdmin.UserPriority = 50;
				pdao.Store(pmAdmin);

				var pmDemo = addMember(project, context.Demo, BaseProjectRoles.Administrator);
				pmDemo.Roles = new[] {BaseProjectRoles.Administrator, TaskProjectRoles.Manager, TaskProjectRoles.Executor};
				pmDemo.CurrentRole = TaskProjectRoles.Manager;
				pdao.Store(pmDemo);

				var pmUser1 = addMember(project, context.User1, BaseProjectRoles.Administrator);
				pmUser1.UserPriority = 25;
				pdao.Store(pmUser1);

				var pmUser2 = addMember(project, context.User2, TaskProjectRoles.Manager);
				pdao.Store(pmUser2);

				var pmUser3 = addMember(project, context.User3, TaskProjectRoles.Executor);
				pdao.Store(pmUser3);

				var pmArtem1 = addMember(project, context.Artem1, BaseProjectRoles.Administrator);
				pmArtem1.UserPriority = 10;
				pdao.Store(pmArtem1);

				var pmOlegSmith = addMember(project, context.OlegSmith, BaseProjectRoles.Administrator);
				pmOlegSmith.UserPriority = 15;
				pdao.Store(pmOlegSmith);

				foreach (var tag in context.AdminTags)
				{
					mainDao.Store(new ProjectToTagModel
					{
						Project = project,
						Tag = tag
					});
				}

				mainDao.Store(project);
				SessionProviderRegistry.GetProjectProvider(project.ProjectCode).CurrentSession.Flush();
				return project;
			};

			return new
			{
				Software = createProject("soft", "Software development project", "This is a flexible project management web application written using angularjs framework."),
				CRM = createProject("crm", "CRM tasks", "Customer relation management tasks"),
				Personal = createProject("personal", "Personal tasks", "Personal tasks project"),
				Helpdesk = createProject("hd", "Helpdesk", "Helpdesk department tasks")
			};
		}

		protected dynamic DoPopulateTaskTypes(dynamic context, dynamic projects)
		{
			Func<string, ProjectModel, TaskTypeModel> factory = (name, proj) =>
			{
				var taskType = new TaskTypeModel
				{
					Creator = UserToMember(proj, context.Admin),
					ProjectCode = proj.ProjectCode,
					Name = name
				};
				ProjectDao(proj.ProjectCode).Store(taskType);
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
					Creator = UserToMember(proj, context.Admin),
					Name = name,
					FullName = name,
					ValueType = type
				};
				ProjectDao(proj.ProjectCode).Store(propertyType);
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

		protected dynamic DoPopulateTags(dynamic context, dynamic projects)
		{
			Func<string, dynamic, UserModel, TaskTagModel> factory = (name, project, owner) =>
			{
			    var tag = new TaskTagModel
			                {
			                    ProjectCode = project.ProjectCode,
								OwnerId = owner.Id,
								Name = name,
								FullName = name
			                };
				ProjectDao(project.ProjectCode).Store(tag);
			    return tag;
			};

			return new
			{
				Software = new
				{
					Refactor = factory("Refactor this", projects.Software, context.Admin),
					NeedsLearning = factory("Needs learning", projects.Software, context.Admin)
				},
				CRM = new
				{
					ABC = factory("ABC", projects.CRM, context.Admin),
					Forget = factory("Forget", projects.CRM, context.Admin)
				},
				Personal = new
				{
					Must = factory("must", projects.Personal, context.Admin),
					Optional = factory("optional", projects.Personal, context.Admin)
				},
				Helpdesk = new
				{
					Level1 = factory("Escalation level 1", projects.Helpdesk, context.Admin),
					Level2 = factory("Escalation level 2", projects.Helpdesk, context.Admin),
					Level3 = factory("Escalation level 3", projects.Helpdesk, context.Admin)
				}
			};
		}

		protected void DoPopulateTasks(dynamic context, dynamic projects, dynamic taskTypes, dynamic propertyTypes, dynamic tags)
		{
			Func<int, TaskTypeModel, TaskStatus, TaskPriority, string, DateTime?, ProjectModel, dynamic[], decimal?, TaskModel> createTask =
				(num, type, status, priority, content, dueDate, proj, users, estimate) =>
			{
				var task = new TaskModel
				{
					Creator = UserToMember(proj, context.Admin),
					ProjectCode = proj.ProjectCode,
					InternalSeqNumber = num,
					SeqNumber = "t0-" + num,
					Priority = priority,
					Content = content,
					DueDate = dueDate,
					TaskType = type,
					EstimatedTime = estimate
				};
				task.ChangeStatus(status, UserToMember(proj, context.Admin));
				foreach (var user in users)
				{
					var executor = new TaskExecutorModel
					{
					    Creator = UserToMember(proj, context.Admin),
					    Task = task,
					    Executor = UserToMember(proj, user)
					};
					task.Executors.Add(executor);
				}
				ProjectDao(proj.ProjectCode).Store(task);
				return task;
			};
			Action<TaskModel, CustomPropertyTypeModel, object> createTaskProperty = (task, propertyType, value) =>
			{
				var taskProperty = new TaskCustomPropertyModel
				{
					Task = task,
					Creator = UserToMember(task, context.Admin),
					PropertyType = propertyType,
					Value = value
				};
				ProjectDao(propertyType.ProjectCode).Store(taskProperty);
			};
			Action<TaskModel, TaskTagModel> addTag = (task, tag) =>
			{
			    var tagLink = new TaskToTagModel
			                    {
									Creator = UserToMember(task, context.Admin),
			                        Task = task,
			                        Tag = tag
			                    };
			    ProjectDao(task.ProjectCode).Store(tagLink);
			};
			Action<TaskModel, UserModel, decimal, string> addTime = (task, user, time, comment) =>
			{
				var entry = new TaskTimelogEntryModel
				{
					Task = task,
					Member = task.Executors.First(e => e.Executor.UserId == user.Id).Executor,
					Time = time,
					Comment = comment
				};
				ProjectDao(task.ProjectCode).Store(entry);
			};

// ReSharper disable UnusedVariable

			//Software
			var seqnum = 0;
			var sp = projects.Software;
			var stt = taskTypes.Software;
			var spt = propertyTypes.Software;
			var stg = tags.Software;
			var st1 = createTask(seqnum++, stt.Feature, TaskStatus.Done, TaskPriority.Low,
				"Workflow configuration", DateTime.Now.AddDays(3), sp, new[] { context.User1, context.User2 }, 4);
			var st2 = createTask(seqnum++, stt.Bug, TaskStatus.New, TaskPriority.High,
				"Ticket subject and text cuting when recieving from E-mail", DateTime.Now.AddDays(1), sp, new[] { context.User1 }, null);
			var st3 = createTask(seqnum++, stt.Feature, TaskStatus.Doing, TaskPriority.Normal,
				"Encrypt emails with SMIME", DateTime.Now.AddDays(-2), sp, new[] { context.User1 }, 12);
			var st4 = createTask(seqnum++, stt.Feature, TaskStatus.Closed, TaskPriority.Normal,
				"Improve usage of label \"button_update\"", DateTime.Now.AddDays(-10), sp, new[] { context.User1 }, 3);
			var st5 = createTask(seqnum++, stt.Support, TaskStatus.Discarded, TaskPriority.Normal,
				"Plugin rollback migration", DateTime.Now.AddDays(-1), sp, new[] { context.User3 }, null);
			var st6 = createTask(seqnum, stt.Bug, TaskStatus.New, TaskPriority.High,
				"Can't move parent ticket between projects", DateTime.Now.AddDays(1), sp, 
				new[] { context.User1, context.User2, context.User3 }, null);
			createTaskProperty(st1, spt.PO, "Jonh O'Connor");
			createTaskProperty(st1, spt.MgrEstimate, 3);
			createTaskProperty(st1, spt.DevEstimate, 5);
			createTaskProperty(st1, spt.LPS, DateTime.Now.AddDays(-1));
			createTaskProperty(st6, spt.PO, "Abigeil Manson");
			createTaskProperty(st6, spt.MgrEstimate, 2);
			createTaskProperty(st6, spt.DevEstimate, 2);
			addTag(st1, stg.Refactor);
			addTag(st1, stg.NeedsLearning);
			addTag(st3, stg.NeedsLearning);
			addTag(st5, stg.Refactor);
			addTime(st1, context.User1, 2, null);
			addTime(st1, context.User2, 2.5m, null);
			addTime(st3, context.User1, 5, "Add bcrypt library and write tests");
			addTime(st4, context.User1, 1.8m, "Add regex to label matcher");

			//CRM
			seqnum = 0;
			var crm = projects.CRM;
			var crmtt = taskTypes.CRM;
			var crmpt = propertyTypes.CRM;
			var ctg = tags.CRM;
			var ct1 = createTask(seqnum++, crmtt.Upselling, TaskStatus.Doing, TaskPriority.Normal,
			                     "Launch test compaign", DateTime.Now.AddDays(7), crm, new[] {context.User2}, 4);
			var ct2 = createTask(seqnum++, crmtt.Audit, TaskStatus.Done, TaskPriority.High,
								 "Prepare for audit", DateTime.Now.AddDays(-2), crm, new[] { context.User1 }, 40);
			var ct3 = createTask(seqnum++, crmtt.Personal, TaskStatus.Closed, TaskPriority.Normal,
								 "Call Mr. Cobson for new bills", DateTime.Now.AddDays(-15), crm, new[] { context.User3 }, 0.5m);
			var ct4 = createTask(seqnum++, crmtt.Upselling, TaskStatus.New, TaskPriority.Low,
								 "Meeting with CEOs", null, crm, new[] { context.User1, context.User3 }, 2.5m);
			var ct5 = createTask(seqnum, crmtt.Upselling, TaskStatus.New, TaskPriority.Normal,
								 "Email partners (confirm $30,000 deal)", DateTime.Now.AddDays(2), crm, new[] { context.User2 }, null);
			createTaskProperty(ct1, crmpt.RelationsLevel, "Good");
			createTaskProperty(ct1, crmpt.Prospects, 2);
			createTaskProperty(ct4, crmpt.RelationsLevel, "Excellent");
			createTaskProperty(ct4, crmpt.Prospects, 5);
			createTaskProperty(ct4, crmpt.BirthDay, new DateTime(1970, 02, 02));
			createTaskProperty(ct5, crmpt.RelationsLevel, "Poor");
			createTaskProperty(ct5, crmpt.LastContact, DateTime.Now.AddDays(-40));
			addTag(ct1, ctg.ABC);
			addTag(ct2, ctg.Forget);
			addTag(ct5, ctg.ABC);
			addTime(ct1, context.User2, 2, null);
			addTime(ct2, context.User1, 52, "Long wait for accounting department shift deadline");
			addTime(ct3, context.User3, 0.7m, null);

			//Personal
			seqnum = 0;
			var pp = projects.Personal;
			var ptt = taskTypes.Personal;
			var ppt = propertyTypes.Personal;
			var ptg = tags.Personal;
			var pt1 = createTask(seqnum++, ptt.Home, TaskStatus.New, TaskPriority.Normal,
			                     "after test, get book to read", null, pp, new[] {context.User2}, null);
			var pt2 = createTask(seqnum++, ptt.Bills, TaskStatus.New, TaskPriority.Normal,
								 "pay car insurance", DateTime.Now.AddDays(26), pp, new[] { context.User2 }, null);
			var pt3 = createTask(seqnum++, ptt.Web, TaskStatus.Doing, TaskPriority.Normal,
								 "stock pictures", null, pp, new[] { context.User2 }, null);
			var pt4 = createTask(seqnum++, ptt.Work, TaskStatus.Closed, TaskPriority.High,
								 "call to office for tomorrow meeting issues", DateTime.Now, pp, new[] { context.User2 }, null);
			var pt5 = createTask(seqnum++, ptt.Home, TaskStatus.Done, TaskPriority.Normal,
								 "plan for hispanohablantes", DateTime.Now.AddDays(-5), pp, new[] { context.User2 }, null);
			var pt6 = createTask(seqnum, ptt.Web, TaskStatus.New, TaskPriority.Normal,
								 "emotion posters", null, pp, new[] { context.User2 }, null);
			createTaskProperty(pt1, ppt.Note, "use my new kindle HD");
			createTaskProperty(pt4, ppt.Deadline, DateTime.Now.AddDays(1));
			addTag(pt1, ptg.Optional);
			addTag(pt2, ptg.Must);
			addTag(pt3, ptg.Optional);
			addTag(pt4, ptg.Must);

			//Helpdesk
			seqnum = 0;
			var hp = projects.Helpdesk;
			var htt = taskTypes.Helpdesk;
			var hpt = propertyTypes.Helpdesk;
			var htg = tags.Helpdesk;
			var ht1 = createTask(seqnum++, htt.Consult, TaskStatus.Doing, TaskPriority.Normal,
			                     "Can't connect to office vpn", DateTime.Now, hp, new[] {context.User2}, 1);
			var ht2 = createTask(seqnum++, htt.Support, TaskStatus.Closed, TaskPriority.Normal,
								 "Remove paper jam from fax on office 314", DateTime.Now, hp, new[] { context.User1 }, 0.5m);
			var ht3 = createTask(seqnum++, htt.Management, TaskStatus.New, TaskPriority.High,
								 "Calculate total work hours at the end of week", DateTime.Now.AddDays(2), hp, new[] { context.Admin }, 0.5m);
			var ht4 = createTask(seqnum++, htt.Test, TaskStatus.New, TaskPriority.Low,
								 "Execute memory test on new laptops 23433 and 23434", null, hp, new[] { context.User1 }, 2m);
			var ht5 = createTask(seqnum++, htt.Consult, TaskStatus.New, TaskPriority.Normal,
								 "Needs help for SAP in accounting department", DateTime.Now.AddDays(1), hp, new[] { context.User3 }, null);
			var ht6 = createTask(seqnum++, htt.Support, TaskStatus.New, TaskPriority.High,
								 "No wifi in meeting room", DateTime.Now, hp, new[] { context.User2 }, 0.8m);
			var ht7 = createTask(seqnum++, htt.Management, TaskStatus.Done, TaskPriority.Normal,
								 "Plan vacations for next year", DateTime.Now.AddDays(-2), hp, new[] { context.Admin }, 15);
			var ht8 = createTask(seqnum, htt.Support, TaskStatus.Closed, TaskPriority.Normal,
								 "Move accounting database backup to NV office", DateTime.Now.AddDays(-6), hp, new[] { context.User3 }, 15);
			createTaskProperty(ht1, hpt.SpentHours, 1.2m);
			createTaskProperty(ht1, hpt.ClientSatisfaction, "normal");
			createTaskProperty(ht3, hpt.SpentHours, 3);
			createTaskProperty(ht5, hpt.SpentHours, 1.2m);
			createTaskProperty(ht5, hpt.ClientSatisfaction, "low");
			createTaskProperty(ht5, hpt.ClientAdequacy, "poor");
			addTag(ht1, htg.Level1);
			addTag(ht2, htg.Level1);
			addTag(ht3, htg.Level3);
			addTag(ht4, htg.Level2);
			addTag(ht5, htg.Level3);
			addTag(ht6, htg.Level1);
			addTag(ht7, htg.Level3);
			addTag(ht8, htg.Level2);
			addTime(ht1, context.User2, 1, "Require sysadmin support, can't fix myself");
			addTime(ht7, context.Admin, 13, null);
			addTime(ht8, context.User3, 14, null);

// ReSharper restore UnusedVariable
		}

		protected void DoPopulateReports(dynamic context, dynamic projects)
		{
			var reports = new TasksReports();
			foreach (var proj in new ProjectModel[] {projects.Software, projects.CRM, projects.Personal, projects.Helpdesk})
			{
				var session = SessionProviderRegistry.GetProjectProvider(proj.ProjectCode).CurrentSession;
				reports.PopulateReports(session, proj.ProjectCode);
				session.Flush();
			}
		}

		#endregion
	}
}
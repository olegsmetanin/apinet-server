using System;
using System.Linq;
using AGO.Core;
using AGO.Core.Controllers;
using AGO.Core.Filters;
using AGO.Core.Model.Security;
using AGO.Core.Security;
using AGO.Tasks.Controllers;
using AGO.Tasks.Controllers.DTO;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace AGO.Tasks.Test.Security
{
	public class TasksControllerSecurityTests: AbstractTest
	{
		private TasksController controller;
		private UserModel admin;
		private UserModel projAdmin;
		private UserModel projManager;
		private UserModel projExecutor;
		private UserModel notMember;

		public override void FixtureSetUp()
		{
			base.FixtureSetUp();

			controller = IocContainer.GetInstance<TasksController>();
		}

		protected override void SetupTestProject()
		{
			admin = LoginToUser("admin@apinet-test.com");
			projAdmin = LoginToUser("user1@apinet-test.com");
			projManager = LoginToUser("user2@apinet-test.com");
			projExecutor = LoginToUser("user3@apinet-test.com");
			notMember = LoginToUser("artem1@apinet-test.com");
			FM.Project(TestProject);
			FM.Member(TestProject, projAdmin, BaseProjectRoles.Administrator);
			FM.Member(TestProject, projManager, TaskProjectRoles.Manager);
			FM.Member(TestProject, projExecutor, TaskProjectRoles.Executor);
		}

		[Test]
		public void OnlyMembersCanLookupTasks()
		{
			M.Task(1, executor: projManager);
			M.Task(2, executor: projExecutor);

			Func<UserModel, LookupEntry[]> action = u =>
			{
				Login(u.Login);
				return controller.LookupTasks(TestProject, null, 0).ToArray();
			};
			ReusableConstraint granted = Has.Length.EqualTo(2)
				.And.Exactly(1).Matches<LookupEntry>(e => e.Text == "t0-1")
				.And.Exactly(1).Matches<LookupEntry>(e => e.Text == "t0-2");
			ReusableConstraint denied = Is.Empty;

			Assert.That(action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanGetTasks()
		{
			M.Task(1, executor: projManager);
			M.Task(2, executor: projExecutor);

			Func<UserModel, TaskListItemDTO[]> action = u =>
			{
				Login(u.Login);
				return controller.GetTasks(TestProject, 
					Enumerable.Empty<IModelFilterNode>().ToList(),
					Enumerable.Empty<SortInfo>().ToList(),
					0, TaskPredefinedFilter.All).ToArray();
			};
			ReusableConstraint granted = Has.Length.EqualTo(2)
				.And.Exactly(1).Matches<TaskListItemDTO>(e => e.SeqNumber == "t0-1")
				.And.Exactly(1).Matches<TaskListItemDTO>(e => e.SeqNumber == "t0-2");
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanGetTasksCount()
		{
			M.Task(1, executor: projManager);
			M.Task(2, executor: projExecutor);

			Func<UserModel, int> action = u =>
			{
				Login(u.Login);
				return controller.GetTasksCount(TestProject,
					Enumerable.Empty<IModelFilterNode>().ToList(),
					TaskPredefinedFilter.All);
			};
			ReusableConstraint granted = Is.EqualTo(2);
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanGetTaskDetails()
		{
			var task = M.Task(1, executor: projManager);

			Func<UserModel, TaskListItemDetailsDTO> action = u =>
			{
				Login(u.Login);
				return controller.GetTaskDetails(TestProject, task.SeqNumber);
			};
			ReusableConstraint granted = Is.Not.Null;
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanGetTask()
		{
			var task = M.Task(1, executor: projManager);

			Func<UserModel, TaskViewDTO> action = u =>
			{
				Login(u.Login);
				return controller.GetTask(TestProject, task.SeqNumber);
			};
			ReusableConstraint granted = Is.Not.Null;
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyAdminOrManagerCanCreateTask()
		{
			var tt = M.TaskType();
			var executorId = M.MemberFromUser(TestProject, projExecutor).Id;
			Func<UserModel, bool> action = u =>
			{
				Login(u.Login);
				var ur = controller.CreateTask(TestProject,
					new CreateTaskDTO {TaskType = tt.Id, Content = "nunit", Executors = new[] {executorId}});
				Session.Flush();
				if (ur.Validation.Success)
				{
					M.Track(Session.QueryOver<TaskModel>()
						.Where(m => m.ProjectCode == TestProject && m.SeqNumber == ur.Model)
						.SingleOrDefault());
				}
				return ur.Validation.Success;
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint denied = Is.False;

			Assert.That(action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), denied);
			Assert.That(action(notMember), denied);
		}

		//TODO property-level security, when implemented, will require more fine grained tests
		[Test]
		public void OnlyMembersCanEditTask()
		{
			var task = M.Task(1, executor:projExecutor);
			Func<UserModel, bool> action = u =>
			{
				Login(u.Login);
				var ur = controller.UpdateTask(TestProject,
					new PropChangeDTO
					{
						Id = task.Id, 
						Prop = "Content", 
						ModelVersion = task.ModelVersion, 
						Value = "nunit"
					});
				return ur.Validation.Success;
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint denied = Is.False;

			Assert.That(action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanAddAgreemer()
		{
			var agreemerId = M.MemberFromUser(TestProject, projManager).Id;
			Func<UserModel, Agreement> action = u =>
			{
				Login(u.Login);
				var task = M.Task(1, executor: projExecutor);
				return controller.AddAgreemer(task.Id, agreemerId);
			};
			ReusableConstraint granted = Is.Not.Null;
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyAdminOrManagerAgreemerCanRemoveAgreement()
		{
			Func<UserModel, UserModel, bool> action = (u, a) =>
			{
				var task = M.Task(1, executor: projExecutor);
				var agreement = M.Agreement(task, a, u);

				Login(u.Login);

				return controller.RemoveAgreement(task.Id, agreement.Id);
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint restricted = Throws.Exception.TypeOf<DeleteDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin, projManager), denied);
			Assert.That(action(projAdmin, projAdmin), granted);
			Assert.That(action(projAdmin, projManager), granted);
			Assert.That(() => action(projManager, projAdmin), restricted);
			Assert.That(action(projManager, projManager), granted);
			Assert.That(() => action(projExecutor, projManager), restricted);
			Assert.That(() => action(projExecutor, projExecutor), restricted);
			Assert.That(() => action(notMember, projAdmin), denied);
		}

		[Test]
		public void OnlyAdminAgreemerOrManagerAgreemerCanAgree()
		{
			Func<UserModel, UserModel, bool> action = (u, a) =>
			{
				var task = M.Task(1, executor: projExecutor);
				M.Agreement(task, a, u);

				Login(u.Login);

				return controller.AgreeTask(task.Id, "nunit").Done;
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint incorrect = Throws.Exception.TypeOf<CurrentUserIsNotAgreemerInTaskException>();
			ReusableConstraint restricted = Throws.Exception.TypeOf<ChangeDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin, projManager), denied);
			Assert.That(action(projAdmin, projAdmin), granted);
			Assert.That(() => action(projAdmin, projManager), incorrect);
			Assert.That(() => action(projManager, projAdmin), incorrect);
			Assert.That(action(projManager, projManager), granted);
			Assert.That(() => action(projExecutor, projManager), incorrect);
			Assert.That(() => action(projExecutor, projExecutor), restricted);
			Assert.That(() => action(notMember, projAdmin), denied);
		}

		[Test]
		public void OnlyAdminAgreemerOrManagerAgreemerCanRevoke()
		{
			Func<UserModel, UserModel, bool> action = (u, a) =>
			{
				var task = M.Task(1, executor: projExecutor);
				M.Agreement(task, a, u, true);

				Login(u.Login);

				return controller.RevokeAgreement(task.Id).Done;
			};
			ReusableConstraint granted = Is.False;
			ReusableConstraint incorrect = Throws.Exception.TypeOf<CurrentUserIsNotAgreemerInTaskException>();
			ReusableConstraint restricted = Throws.Exception.TypeOf<ChangeDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin, projManager), denied);
			Assert.That(action(projAdmin, projAdmin), granted);
			Assert.That(() => action(projAdmin, projManager), incorrect);
			Assert.That(() => action(projManager, projAdmin), incorrect);
			Assert.That(action(projManager, projManager), granted);
			Assert.That(() => action(projExecutor, projManager), incorrect);
			Assert.That(() => action(projExecutor, projExecutor), restricted);
			Assert.That(() => action(notMember, projAdmin), denied);
		}

		[Test]
		public void OnlyAdminOrManagerCanDeleteTask()
		{
			Func<UserModel, bool> action = u =>
			{
				var task = M.Task(1, executor: projExecutor);
				Login(u.Login);
				return controller.DeleteTask(task.Id);
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint restricted = Throws.Exception.TypeOf<DeleteDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(() => action(projExecutor), restricted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanLookupParamTypes()
		{
			M.ParamType("p1");
			M.ParamType("p2");

			Func<UserModel, CustomParameterTypeDTO[]> action = u =>
			{
				Login(u.Login);
				return controller.LookupParamTypes(TestProject, null, 0).ToArray();
			};
			ReusableConstraint granted = Has.Length.EqualTo(2)
				.And.Exactly(1).Matches<CustomParameterTypeDTO>(e => e.Text == "p1")
				.And.Exactly(1).Matches<CustomParameterTypeDTO>(e => e.Text == "p2");
			ReusableConstraint denied = Is.Empty;

			Assert.That(action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanManageParams()
		{
			var task = M.Task(1, executor: projExecutor);
			var pt = M.ParamType();
			Func<UserModel, bool> action = u =>
			{
				Login(u.Login);
				var ur1 = controller.EditParam(task.Id,
					new CustomParameterDTO
					{
						Type = new CustomParameterTypeDTO {Id = pt.Id},
						Value = "nunit"
					});
				Session.Flush();
				var dto = ur1.Model;
				dto.Value = "nunit changed";
				var ur2 = controller.EditParam(task.Id, dto);
				Session.Flush();
				var r3 = controller.DeleteParam(ur2.Model.Id);
				Session.Flush();
				return ur1.Validation.Success && ur2.Validation.Success && r3;
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanTagTask()
		{
			var adminTag = M.Tag("adminTag", owner: projAdmin);
			var mgrTag = M.Tag("mgrTag", owner: projManager);
			var execTag = M.Tag("execTag", owner: projExecutor);
			Func<UserModel, TaskTagModel, bool> action = (u, t) =>
			{
				var task = M.Task(1, executor: projExecutor);
				
				Login(u.Login);
				var result = controller.TagTask(task.Id, t.Id);
				Session.Flush();
				return result;
			};
			
			ReusableConstraint granted = Is.True;
			ReusableConstraint restricted = Throws.Exception.TypeOf<CreationDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin, mgrTag), denied);

			Assert.That(action(projAdmin, adminTag), granted);
			Assert.That(() => action(projAdmin, mgrTag), restricted);

			Assert.That(action(projManager, mgrTag), granted);
			Assert.That(() => action(projManager, adminTag), restricted);

			Assert.That(action(projExecutor, execTag), granted);
			Assert.That(() => action(projExecutor, adminTag), restricted);
			
			Assert.That(() => action(notMember, adminTag), denied);
		}

		[Test]
		public void OnlyMembersCanDetagTask()
		{

			var adminTag = M.Tag("adminTag", owner: projAdmin);
			var mgrTag = M.Tag("mgrTag", owner: projManager);
			var execTag = M.Tag("execTag", owner: projExecutor);
			Func<UserModel, TaskTagModel, bool> action = (u, t) =>
			{
				var task = M.Task(1, executor: projExecutor);
				var link = new TaskToTagModel
				{
					Creator = u,
					Task = task,
					Tag = t
				};
				task.Tags.Add(link);
				Session.Save(link);
				Session.Flush();
				Session.Clear();//needs for cascade operation

				Login(u.Login);
				var result = controller.DetagTask(task.Id, t.Id);
				Session.Flush();
				return result;
			};

			ReusableConstraint granted = Is.True;
			ReusableConstraint restricted = Throws.Exception.TypeOf<DeleteDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin, execTag), denied);

			Assert.That(action(projAdmin, adminTag), granted);
			Assert.That(() => action(projAdmin, execTag), restricted);

			Assert.That(action(projManager, mgrTag), granted);
			Assert.That(() => action(projManager, adminTag), restricted);

			Assert.That(action(projExecutor, execTag), granted);
			Assert.That(() => action(projExecutor, mgrTag), restricted);

			Assert.That(() => action(notMember, execTag), denied);
		}
	}
}
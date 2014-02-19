using System;
using System.Linq;
using AGO.Core;
using AGO.Core.Controllers;
using AGO.Core.Filters;
using AGO.Core.Model.Security;
using AGO.Core.Security;
using AGO.Tasks.Controllers.DTO;
using AGO.Tasks.Model.Dictionary;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace AGO.Tasks.Test.Security
{
	public class DictionaryControllerSecurityTests: AbstractDictionaryTest
	{
		private UserModel admin;
		private UserModel projAdmin;
		private UserModel projManager;
		private UserModel projExecutor;
		private UserModel notMember;

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
		public void OnlyMembersCanLookupTaskTypes()
		{
			M.TaskType("aaa");
			M.TaskType("bbb");

			Func<UserModel, LookupEntry[]> action = u =>
			{
				Login(u.Login);
				return Controller.LookupTaskTypes(TestProject, null, 0).ToArray();
			};
			ReusableConstraint granted = Has.Length.EqualTo(2)
				.And.Exactly(1).Matches<LookupEntry>(e => e.Text == "aaa")
				.And.Exactly(1).Matches<LookupEntry>(e => e.Text == "bbb");
			ReusableConstraint denied = Is.Empty;

			Assert.That(action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanGetTaskTypes()
		{
			var aaa = M.TaskType("aaa");
			var bbb = M.TaskType("bbb");

			Func<UserModel, TaskTypeDTO[]> action = u =>
			{
				Login(u.Login);
				return Controller.GetTaskTypes(
					TestProject, 
					Enumerable.Empty<IModelFilterNode>().ToList(),
					Enumerable.Empty<SortInfo>().ToList(), 
					0).ToArray();
			};
			ReusableConstraint granted = Has.Length.EqualTo(2)
				.And.Exactly(1).Matches<TaskTypeDTO>(dto => dto.Id == aaa.Id)
				.And.Exactly(1).Matches<TaskTypeDTO>(dto => dto.Id == bbb.Id);
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanGetTaskTypesCount()
		{
			M.TaskType("aaa");
			M.TaskType("bbb");

			Func<UserModel, int> action = u =>
			{
				Login(u.Login);
				return Controller.GetTaskTypesCount(
					TestProject,
					Enumerable.Empty<IModelFilterNode>().ToList());
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
		public void OnlyProjectAdminOrManagerCanCreateTaskType()
		{
			Func<UserModel, UpdateResult<TaskTypeDTO>> action = u =>
			{
				Login(u.Login);
				var ur = Controller.EditTaskType(
					TestProject,
					new TaskTypeDTO {Name = u.Id.ToString()});
				if (ur.Model != null)
				{
					M.Track(Session.Get<TaskTypeModel>(ur.Model.Id));
				}
				return ur;
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint denied = Is.False;

			Assert.That(action(admin).Validation.Success, denied);
			Assert.That(action(projAdmin).Validation.Success, granted);
			Assert.That(action(projManager).Validation.Success, granted);
			Assert.That(action(projExecutor).Validation.Success, denied);
			Assert.That(action(notMember).Validation.Success, denied);
		}

		[Test]
		public void OnlyProjectAdminOrManagerCanEditTaskType()
		{
			var tt = M.TaskType("aaa");

			Func<UserModel, UpdateResult<TaskTypeDTO>> action = u =>
			{
				Login(u.Login);
				var ur = Controller.EditTaskType(
					TestProject,
					new TaskTypeDTO { Id = tt.Id, Name = u.Id.ToString() });
				return ur;
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint denied = Is.False;

			Assert.That(action(admin).Validation.Success, denied);
			Assert.That(action(projAdmin).Validation.Success, granted);
			Assert.That(action(projManager).Validation.Success, granted);
			Assert.That(action(projExecutor).Validation.Success, denied);
			Assert.That(action(notMember).Validation.Success, denied);
		}

		[Test]
		public void OnlyProjectAdminOrManagerCanDeleteTaskType()
		{
			Func<UserModel, bool> action = u =>
			{
				var tt = M.TaskType("for delete");
				Login(u.Login);
				return Controller.DeleteTaskType(tt.Id);
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
		 
	}
}
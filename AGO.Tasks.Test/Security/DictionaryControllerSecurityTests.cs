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
using DictionaryController = AGO.Tasks.Controllers.DictionaryController;

namespace AGO.Tasks.Test.Security
{
	public class DictionaryControllerSecurityTests: AbstractSecurityTest
	{
		private DictionaryController controller;

		public override void FixtureSetUp()
		{
			base.FixtureSetUp();

			controller = IocContainer.GetInstance<DictionaryController>();
		}

		#region Task type

		[Test]
		public void OnlyMembersCanLookupTaskTypes()
		{
			M.TaskType("aaa");
			M.TaskType("bbb");

			Func<UserModel, LookupEntry[]> action = u =>
			{
				Login(u.Login);
				return controller.LookupTaskTypes(TestProject, null, 0).ToArray();
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
				return controller.GetTaskTypes(
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
				return controller.GetTaskTypesCount(
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
				var ur = controller.EditTaskType(
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
				return controller.EditTaskType(
					TestProject,
					new TaskTypeDTO { Id = tt.Id, Name = u.Id.ToString() });
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
				return controller.DeleteTaskType(tt.Id);
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

		#endregion

		#region Task tag

		[Test]
		public void OnlyMembersCanLookupOwnTags()
		{
			var admintTag = M.Tag(projAdmin.Login, owner: projAdmin);
			var mgrTag = M.Tag(projManager.Login, owner: projManager);
			var execTag = M.Tag(projExecutor.Login, owner: projExecutor);

			Func<UserModel, LookupEntry[]> action = u =>
			{
				Login(u.Login);
				return controller.LookupTags(TestProject, null, 0).ToArray();
			};
			Func<TaskTagModel, ReusableConstraint> granted = tag => Has.Length.EqualTo(1)
				.And.Exactly(1).Matches<LookupEntry>(e => e.Text == tag.FullName);
			ReusableConstraint denied = Is.Empty;

			Assert.That(action(admin), denied);
			Assert.That(action(projAdmin), granted(admintTag));
			Assert.That(action(projManager), granted(mgrTag));
			Assert.That(action(projExecutor), granted(execTag));
			Assert.That(action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanGetOwnTags()
		{
			var admintTag = M.Tag(projAdmin.Login, owner: projAdmin);
			var mgrTag = M.Tag(projManager.Login, owner: projManager);
			var execTag = M.Tag(projExecutor.Login, owner: projExecutor);

			Func<UserModel, TaskTagDTO[]> action = u =>
			{
				Login(u.Login);
				return controller.GetTags(TestProject,
					Enumerable.Empty<IModelFilterNode>().ToList(),
					Enumerable.Empty<SortInfo>().ToList(),
					0).ToArray();
			};
			Func<TaskTagModel, ReusableConstraint> granted = tag => Has.Length.EqualTo(1)
				.And.Exactly(1).Matches<TaskTagDTO>(e => e.Name == tag.FullName);
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted(admintTag));
			Assert.That(action(projManager), granted(mgrTag));
			Assert.That(action(projExecutor), granted(execTag));
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanCreateOwnTags()
		{
			Func<UserModel, bool> action = u =>
			{
				Login(u.Login);
				var ur = controller.CreateTag(TestProject, new TaskTagDTO { Name = u.Id.ToString() });
				if (ur.Validation.Success)
				{
					M.Track(Session.Get<TaskTagModel>(ur.Model[0].Id));
				}
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
		public void OnlyMembersCanEditOwnTags()
		{
			Func<UserModel, bool> action = u =>
			{
				var utag = M.Tag(u.Id.ToString(), owner: u);
				Login(u.Login);
				return controller.EditTag(TestProject, new TaskTagDTO { Id = utag.Id, Name = "aaa" })
					.Validation.Success;
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
		public void OnlyMembersCanDeleteOwnTags()
		{
			var admintTag = M.Tag(projAdmin.Login, owner: projAdmin);
			var mgrTag = M.Tag(projManager.Login, owner: projManager);
			var execTag = M.Tag(projExecutor.Login, owner: projExecutor);

			Func<UserModel, Guid, Guid[]> action = (u, id) =>
			{				
				Login(u.Login);
				return controller.DeleteTags(TestProject, new []{ id }).ToArray();
			};
			Func<Guid, ReusableConstraint> granted = id => Has.Length.EqualTo(1)
				.And.Exactly(1).Matches<Guid>(i => i == id);
			ReusableConstraint restricted = Throws.Exception.TypeOf<DeleteDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			//not project members
			Assert.That(() => action(admin, mgrTag.Id), denied);
			Assert.That(() => action(notMember, execTag.Id), denied);
			//not own tags
			Assert.That(() => action(projAdmin, mgrTag.Id), restricted);
			Assert.That(() => action(projManager, execTag.Id), restricted);
			Assert.That(() => action(projExecutor, admintTag.Id), restricted);
			//own tags
			Assert.That(action(projAdmin, admintTag.Id), granted(admintTag.Id));
			Assert.That(action(projManager, mgrTag.Id), granted(mgrTag.Id));
			Assert.That(action(projExecutor, execTag.Id), granted(execTag.Id));
		}

		#endregion
	}
}
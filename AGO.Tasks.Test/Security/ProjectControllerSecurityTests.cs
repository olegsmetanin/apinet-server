using System;
using System.Linq;
using AGO.Core;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Core.Security;
using AGO.Tasks.Controllers;
using AGO.Tasks.Controllers.DTO;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace AGO.Tasks.Test.Security
{
	public class ProjectControllerSecurityTests: AbstractSecurityTest
	{
		private ProjectController controller;

		public override void FixtureSetUp()
		{
			base.FixtureSetUp();

			controller = IocContainer.GetInstance<ProjectController>();
		}

		[Test]
		public void OnlySysAdminOrMembersCanGetProject()
		{
			Func<UserModel, ProjectDTO> action = u =>
			{
				Login(u.Email);
				return controller.GetProject(TestProject);
			};
			ReusableConstraint granted = Is.Not.Null;
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectException>();

			Assert.That(action(admin), granted);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyProjectAdminCanUpdateProject()
		{
			Func<UserModel, bool> action = u =>
			{
				Login(u.Email);
				var proj = FM.ProjectFromCode(TestProject);
				var ur = controller.UpdateProject(TestProject,
					new PropChangeDTO
					{
						Id = proj.Id,
						ModelVersion = proj.ModelVersion,
						Prop = "Description",
						Value = "NUnit"
					});
				return ur.Validation.Success;
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint denied = Is.False;

			Assert.That(action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), denied);
			Assert.That(action(projExecutor), denied);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyProjectMembersCanTagProjectWithOwnTag()
		{
			var admTag = MM.ProjectTag("adm", projAdmin);
			var mgrTag = MM.ProjectTag("mgr", projManager);
			var execTag = MM.ProjectTag("exec", projExecutor);
			var proj = FM.ProjectFromCode(TestProject);

			Func<UserModel, ProjectTagModel, bool> action = (u, t) =>
			{
				Login(u.Email);
				return controller.TagProject(proj.Id, t.Id);
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint denied = Throws.Exception.TypeOf<CreationDeniedException>();

			Assert.That(() => action(admin, admTag), denied);

			Assert.That(action(projAdmin, admTag), granted);
			Assert.That(() => action(projAdmin, mgrTag), denied);

			Assert.That(action(projManager, mgrTag), granted);
			Assert.That(()=> action(projManager, execTag), denied);

			Assert.That(action(projExecutor, execTag), granted);
			Assert.That(() => action(projExecutor, admTag), denied);

			Assert.That(() => action(notMember, mgrTag), denied);
		}

		[Test]
		public void OnlyProjectAdminCanDetagProjectWithOwnTag()
		{
			var admTag = MM.ProjectTag("adm", projAdmin);
			var mgrTag = MM.ProjectTag("mgr", projManager);
			var execTag = MM.ProjectTag("exec", projExecutor);
			var proj = FM.ProjectFromCode(TestProject);

			Func<UserModel, ProjectTagModel, bool> action = (u, t) =>
			{
				
				var link = new ProjectToTagModel
				{
					Project = proj,
					Tag = t
				};
				proj.Tags.Add(link);
				MainSession.Save(link);
				MainSession.Flush();
				MainSession.Clear();
				
				Login(u.Email);
				return controller.DetagProject(proj.Id, t.Id);
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint rejected = Is.False;

			Assert.That(() => action(admin, admTag), rejected);

			Assert.That(action(projAdmin, admTag), granted);
			Assert.That(action(projAdmin, mgrTag), rejected);

			Assert.That(action(projManager, mgrTag), granted);
			Assert.That(action(projManager, execTag), rejected);

			Assert.That(action(projExecutor, execTag), granted);
			Assert.That(action(projExecutor, admTag), rejected);

			Assert.That(() => action(notMember, mgrTag), rejected);
		}

		[Test]
		public void OnlyMembersCanGetMembers()
		{
			Func<UserModel, ProjectMemberDTO[]> action = u =>
			{
				Login(u.Email);
				return controller.GetMembers(TestProject, null, 0).ToArray();
			};
			ReusableConstraint granted = Is.Not.Empty;
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanGetMembersCount()
		{
			Func<UserModel, int> action = u =>
			{
				Login(u.Email);
				return controller.GetMembersCount(TestProject, null);
			};
			ReusableConstraint granted = Is.GreaterThan(0);
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyProjectAdminCanAddMember()
		{
			Func<UserModel, UserModel, ProjectMemberDTO> action = (u, mb) =>
			{
				try
				{
					Login(u.Email);

					var dto = controller.AddMember(TestProject, mb.Id, new[] {TaskProjectRoles.Executor});

					MainSession.Flush();
					Session.Flush();
					return dto;
				}
				finally
				{
					MainSession.Clear();
					Session.Clear();

					var proj = FM.ProjectFromCode(TestProject);
					var testMembership = proj.Members.FirstOrDefault(m => m.User.Id == mb.Id);
					var testMember = M.MemberFromUser(TestProject, mb);
					if (testMembership != null)
					{
						proj.Members.Remove(testMembership);
						MainSession.Delete(testMembership);
					}
					if (testMember != null)
					{
						Session.Delete(testMember);
					}
					MainSession.Flush();
					Session.Flush();
					MainSession.Clear();
					Session.Clear();
				}
			};
			ReusableConstraint granted = Is.Not.Null;
			ReusableConstraint restricted = Throws.Exception.TypeOf<CreationDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin, notMember), denied);
			Assert.That(action(projAdmin, notMember), granted);
			Assert.That(() => action(projManager, notMember), restricted);
			Assert.That(() => action(projExecutor, notMember), restricted);
			Assert.That(() => action(notMember, notMember), denied);
		}

		[Test]
		public void OnlyProjectAdminCanRemoveMember()
		{
			
			Action<UserModel, UserModel> action = (u, mb) =>
			{
				try
				{
					var proj = FM.ProjectFromCode(TestProject);
					var newMember = M.Member(proj, mb);
					MainSession.Update(proj);
					MainSession.Flush();
					MainSession.Clear();
					Session.Clear();

					Login(u.Email);

					controller.RemoveMember(TestProject, newMember.Id);

					MainSession.Flush();
					Session.Flush();
				}
				finally
				{
					MainSession.Clear();
					Session.Clear();

					var proj = FM.ProjectFromCode(TestProject);
					var testMembership = proj.Members.FirstOrDefault(m => m.User.Id == mb.Id);
					var testMember = M.MemberFromUser(TestProject, mb);
					if (testMembership != null)
					{
						proj.Members.Remove(testMembership);
						MainSession.Delete(testMembership);
					}
					if (testMember != null)
					{
						Session.Delete(testMember);
					}
					MainSession.Flush();
					Session.Flush();
					MainSession.Clear();
					Session.Clear();
				}
			};
			ReusableConstraint granted = Throws.Nothing;
			ReusableConstraint restricted = Throws.Exception.TypeOf<DeleteDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin, notMember), denied);
			Assert.That(() => action(projAdmin, notMember), granted);
			Assert.That(() => action(projManager, notMember), restricted);
			Assert.That(() => action(projExecutor, notMember), restricted);
			Assert.That(() => action(notMember, admin), denied);
		}

		[Test]
		public void OnlyProjectAdminCanChangeMemberRoles()
		{
			Func<UserModel, UserModel, bool> action = (u, mb) =>
			{
				var member = M.MemberFromUser(TestProject, mb);
				Login(u.Email);
				var ur = controller.ChangeMemberRoles(TestProject, member.Id, member.Roles);
				return ur.Validation.Success;
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint denied = Is.False;

			Assert.That(action(admin, projManager), denied);
			Assert.That(action(projAdmin, projExecutor), granted);
			Assert.That(action(projManager, projExecutor), denied);
			Assert.That(action(projExecutor, projManager), denied);
			Assert.That(action(notMember, projAdmin), denied);
		}

		[Test]
		public void OnlyProjectAdminOrThemselfCanChangeCurrentRole()
		{
			Func<UserModel, UserModel, bool> action = (u, mb) =>
			{
				var member = M.MemberFromUser(TestProject, mb);
				Login(u.Email);
				var ur = controller.ChangeMemberCurrentRole(TestProject, member.Id, member.CurrentRole);
				return ur.Validation.Success;
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint denied = Is.False;

			//Not members
			Assert.That(action(admin, projManager), denied);
			Assert.That(action(notMember, projAdmin), denied);
			
			//change for other
			Assert.That(action(projAdmin, projExecutor), granted);
			Assert.That(action(projManager, projExecutor), denied);
			Assert.That(action(projExecutor, projManager), denied);

			//change for themself
			Assert.That(action(projAdmin, projAdmin), granted);
			Assert.That(action(projManager, projManager), granted);
			Assert.That(action(projExecutor, projExecutor), granted);
		}
	}
}
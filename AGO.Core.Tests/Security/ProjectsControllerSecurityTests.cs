using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using AGO.Core.Controllers;
using AGO.Core.Controllers.Projects;
using AGO.Core.Filters;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Core.Security;
using NUnit.Framework;

namespace AGO.Core.Tests.Security
{
	public class ProjectsControllerSecurityTests: AbstractPersistenceTest<ModelHelper>
	{
		private UserModel admin;
		private UserModel member;
		private UserModel notMember;
		private ProjectsController controller;

		public override void FixtureSetUp()
		{
			base.FixtureSetUp();

			admin = LoginAdmin();
			member = LoginToUser("user1@apinet-test.com");
			notMember = LoginToUser("user2@apinet-test.com");

			controller = IocContainer.GetInstance<ProjectsController>();
		}

		protected override void CreateModelHelpers()
		{
			FM = new ModelHelper(() => Session, () => CurrentUser);
			M = new ModelHelper(() => Session, () => CurrentUser);
		}

		private dynamic MakeProjectsForTest()
		{
			var ptype = M.MakeProjectType();
			var pub = M.MakeProject("public", ptype.Name, pub: true);
			M.MakeMember(pub.ProjectCode, member);
			var priv = M.MakeProject("private", ptype.Name);
			M.MakeMember(priv.ProjectCode, member);

			return new {pub, priv};
		}

		private IEnumerable<ProjectViewModel> DoGetProjectsTest(UserModel user)
		{
			Login(user.Login);

			var testProjFilter = new[] {_FilteringService.Filter<ProjectModel>()
				.WhereString(m => m.Name).Like("NUnit", appendWildcard: true)};
			var data = controller.GetProjects(0,
				testProjFilter,
				Enumerable.Empty<SortInfo>().ToList(),
				ProjectsRequestMode.All);

			Assert.That(data, Is.Not.Null);

			return data;
		}

		private void DoGetProjectsCountTest(UserModel user, int expected)
		{
			Login(user.Login);

			var testProjFilter = new[] {_FilteringService.Filter<ProjectModel>()
				.WhereString(m => m.Name).Like("NUnit", appendWildcard: true)};
			var count = controller.GetProjectsCount(
				testProjFilter,
				ProjectsRequestMode.All);

			Assert.That(count, Is.EqualTo(expected));
		}

		[Test]
		public void AdminGetAllProjects()
		{
			var ctx = MakeProjectsForTest();

			var data = DoGetProjectsTest(admin);

			Assert.That(data.Count(), Is.EqualTo(2));
			Assert.That(data, Has.Exactly(1).Matches<ProjectViewModel>(p => p.Id == ctx.pub.Id));
			Assert.That(data, Has.Exactly(1).Matches<ProjectViewModel>(p => p.Id == ctx.priv.Id));
		}

		[Test]
		public void MemberGetAllVisibleAndParticipatedProjects()
		{
			var ctx = MakeProjectsForTest();

			var data = DoGetProjectsTest(member);

			Assert.That(data.Count(), Is.EqualTo(2));
			Assert.That(data, Has.Exactly(1).Matches<ProjectViewModel>(p => p.Id == ctx.pub.Id));
			Assert.That(data, Has.Exactly(1).Matches<ProjectViewModel>(p => p.Id == ctx.priv.Id));
		}

		[Test]
		public void NotMemberGetOnlyAllVisibleProjects()
		{
			var ctx = MakeProjectsForTest();

			var data = DoGetProjectsTest(notMember);

			Assert.That(data.Count(), Is.EqualTo(1));
			Assert.That(data, Has.Exactly(1).Matches<ProjectViewModel>(p => p.Id == ctx.pub.Id));
		}

		[Test]
		public void AdminCountAllProjects()
		{
			MakeProjectsForTest();

			DoGetProjectsCountTest(admin, 2);
		}

		[Test]
		public void MemberCountAllVisibleAndParticipatedProjects()
		{
			MakeProjectsForTest();

			DoGetProjectsCountTest(member, 2);
		}

		[Test]
		public void NotMemberCountOnlyAllVisibleProjects()
		{
			MakeProjectsForTest();

			DoGetProjectsCountTest(notMember, 1);
		}

		[Test]
		public void AdminCanCreateProject()
		{
			var ptype = M.MakeProjectType();
			var project = M.Track(() =>
			{
				var data = new ProjectModel
				{
					ProjectCode = "NUnit_admin_create",
					Name = "NUnit test",
					TypeId = ptype.Id
				};
				LoginAdmin();
				var response = controller.CreateProject(data, new HashSet<Guid>());
				Session.Flush();
				return response as ProjectModel;
			});

			Assert.That(project, Is.Not.Null);
			Assert.That(project.ProjectCode, Is.EqualTo("NUnit_admin_create"));
		}

		[Test]
		public void NotAdminCanNotCreateProject()
		{
			var ptype = M.MakeProjectType();
			var data = new ProjectModel
			{
				ProjectCode = "NUnit_member_create",
				Name = "NUnit test",
				TypeId = ptype.Id
			};
			Login(member.Login);
			Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
			var response = controller.CreateProject(data, new HashSet<Guid>());
			Session.Clear();//rollback as in action executor
			
			Assert.That(response, Is.Not.Null);
			Assert.That(response, Is.TypeOf<ValidationResult>());
			var validation = (ValidationResult) response;
			Assert.That(validation.Success, Is.False);
			Assert.That(validation.Errors, Has.Exactly(1).Matches("Not enough permissions for create model"));
		}

		private void DoTagProjectTest(UserModel user, bool addAsMember = false)
		{
			var ptype = M.MakeProjectType();
			var project = M.MakeProject("NUnit_admin_proj", ptype.Name);
			if (addAsMember)
			{
				M.MakeMember(project.ProjectCode, user);
			}
			var tag = Session.QueryOver<ProjectTagModel>().List().Take(1).First();

			Login(user.Login);
			var response = controller.TagProject(project.Id, tag.Id);
			Session.Flush();

			Assert.That(response, Is.True);
			Session.Refresh(project);
			Assert.That(project.Tags, Has.Exactly(1).Matches<ProjectToTagModel>(l => l.Tag.Id == tag.Id));
		}

		[Test]
		public void AdminCanTagProject()
		{
			DoTagProjectTest(admin);
		}

		[Test]
		public void MemberCanTagProject()
		{
			DoTagProjectTest(member, true);
		}

		[Test]
		public void NotMemberCanNotTagProject()
		{
			Assert.That(() => DoTagProjectTest(member),
				Throws.Exception.TypeOf<CreationDeniedException>());
		}

		private void DoDetagProjectTest(UserModel user, bool addAsMember = false)
		{
			var ptype = M.MakeProjectType();
			var project = M.MakeProject("NUnit_admin_proj", ptype.Name);
			if (addAsMember)
			{
				M.MakeMember(project.ProjectCode, user);
			}
			var tag = Session.QueryOver<ProjectTagModel>().List().Take(1).First();
			var link = new ProjectToTagModel
			{
				Creator = user,
				Project = project,
				Tag = tag
			};
			project.Tags.Add(link);
			Session.Save(link);
			Session.Save(project);
			Session.Flush();
			Session.Clear();//if omit this, will be exception "object will be re-saved on cascade", can't fix other way

			Login(user.Login);
			var response = controller.DetagProject(project.Id, tag.Id);
			Session.Flush();

			Assert.That(response, Is.True);
			project = Session.Get<ProjectModel>(project.Id);
			Assert.That(project.Tags, Is.Empty);
		}

		[Test]
		public void AdminCanDetagProject()
		{
			DoDetagProjectTest(admin);
		}

		[Test]
		public void MemberCanDetagProject()
		{
			DoDetagProjectTest(admin, true);
		}

		[Test]
		public void NotMemberCanNotDetagProject()
		{
			Assert.That(() => DoDetagProjectTest(notMember),
				Throws.Exception.TypeOf<DeleteDeniedException>());
		}
	}
}
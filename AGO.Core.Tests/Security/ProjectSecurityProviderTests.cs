using System;
using System.Linq;
using AGO.Core.Filters;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Core.Security;
using AGO.Core.Security.Providers;
using NHibernate.Criterion;
using NUnit.Framework;

namespace AGO.Core.Tests.Security
{
	public class ProjectSecurityProviderTests: AbstractPersistenceTest<ModelHelper>
	{
		private ISecurityConstraintsProvider provider;
		private UserModel admin;
		private UserModel member;
		private UserModel notMember;
		private ProjectModel testProject;

		public override void FixtureSetUp()
		{
			base.FixtureSetUp();

			provider = IocContainer.GetAllInstances<ISecurityConstraintsProvider>().First(p => p is ProjectSecurityProvider);
			admin = LoginAdmin();
			member = LoginToUser("user1@apinet-test.com");
			notMember = LoginToUser("user2@apinet-test.com");

			testProject = FM.Project("proj");
			FM.Membership(testProject.ProjectCode, member);
		}

		protected override void CreateModelHelpers()
		{
			FM = new ModelHelper(() => MainSession, () => CurrentUser);
			//M = new ModelHelper(() => Session, () => CurrentUser); not needed
		}

		[Test]
		public void AdminViewAll()
		{
			var filter = provider.ReadConstraint(null, admin.Id, MainSession);
			Assert.That(filter, Is.Null);
		}

		private void TestNotAdminReadConstraint(Guid userId)
		{
			var filter = provider.ReadConstraint(null, userId, MainSession);
			Assert.That(filter, Is.Not.Null);
			Assert.That(filter.Items, Is.All.AssignableTo<IFilterNode>());
			Assert.That(filter.Items.Count(), Is.EqualTo(2));
			Assert.That(filter.Operator, Is.EqualTo(ModelFilterOperators.Or));

			var byVisibility = (IValueFilterNode)filter.Items.First(f => f is IValueFilterNode);
			var byMembership = (IModelFilterNode)filter.Items.First(f => f is IModelFilterNode);
			var visibleForAll = Projections.Property<ProjectModel>(m => m.VisibleForAll).PropertyName;
			var members = Projections.Property<ProjectModel>(m => m.Members).PropertyName;
			var user = Projections.Property<ProjectMembershipModel>(m => m.User).PropertyName;
			var id = Projections.Property<UserModel>(m => m.Id).PropertyName;

			Assert.That(byVisibility, Is.Not.Null);
			Assert.That(byVisibility.Path, Is.EqualTo(visibleForAll));
			Assert.That(byVisibility.Operator, Is.EqualTo(ValueFilterOperators.Eq));
			Assert.That(byVisibility.Operand, Is.EqualTo(bool.TrueString));

			Assert.That(byMembership, Is.Not.Null);
			Assert.That(byMembership.Path, Is.EqualTo(members));
			Assert.That(byMembership.Items.First(), Is.AssignableTo<IModelFilterNode>());
			Assert.That(byMembership.Items.First().Path, Is.EqualTo(user));
			var byUserId = (IValueFilterNode)((IModelFilterNode)byMembership.Items.First()).Items.First();
			Assert.That(byUserId.Path, Is.EqualTo(id));
			Assert.That(byUserId.Operator, Is.EqualTo(ValueFilterOperators.Eq));
			Assert.That(byUserId.Operand, Is.EqualTo(userId.ToString()));
		}

		[Test]
		public void MemberViewVisibleOrWhereParticipated()
		{
			TestNotAdminReadConstraint(member.Id);
		}

		[Test]
		public void NotMemberViewVisibleOrWhereParticipated()
		{
			TestNotAdminReadConstraint(notMember.Id);
		}

		[Test]
		public void AdminCanChangeAll()
		{
			Assert.That(provider.CanCreate(testProject, null, admin.Id, MainSession), Is.True);
			Assert.That(provider.CanUpdate(testProject, null, admin.Id, MainSession), Is.True);
			Assert.That(provider.CanDelete(testProject, null, admin.Id, MainSession), Is.True);
		}

		[Test]
		public void MemberCanOnlyChange()
		{
			Assert.That(provider.CanCreate(testProject, null, member.Id, MainSession), Is.False);
			Assert.That(provider.CanUpdate(testProject, null, member.Id, MainSession), Is.True);
			Assert.That(provider.CanDelete(testProject, null, member.Id, MainSession), Is.False);
		}

		[Test]
		public void NotMemberCanNotChangeAny()
		{
			Assert.That(provider.CanCreate(testProject, null, notMember.Id, MainSession), Is.False);
			Assert.That(provider.CanUpdate(testProject, null, notMember.Id, MainSession), Is.False);
			Assert.That(provider.CanDelete(testProject, null, notMember.Id, MainSession), Is.False);
		}
	}
}
using System;
using AGO.Core.Tests;

namespace AGO.Tasks.Test
{
	public class AbstractTest : AbstractPersistenceTest<ModelHelper>
	{
		protected string TestProject { get; private set; }

		public override void FixtureSetUp()
		{
			TestProject = Guid.NewGuid().ToString().Replace("-", string.Empty);//user in creating model helpers
			
			base.FixtureSetUp();

			SetupTestProject();
		}

		private void SetupTestProject()
		{
			var admin = LoginAdmin();
			FM.Project(TestProject);
			FM.Member(TestProject, admin, TaskProjectRoles.Manager);
		}

		protected override void CreateModelHelpers()
		{
			FM = new ModelHelper(() => Session, () => CurrentUser, TestProject);
			M = new ModelHelper(() => Session, () => CurrentUser, TestProject);
		}
	}
}
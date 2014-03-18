using System;
using AGO.Core;
using AGO.Core.Tests;
using NHibernate;

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

			ProjDao = DaoFactory.CreateProjectCrudDao(TestProject);
		}

		protected virtual void SetupTestProject()
		{
			var admin = LoginAdmin();
			var proj = FM.Project(TestProject);
			M.Member(proj, admin, TaskProjectRoles.Manager);
		}

		protected override void CreateModelHelpers()
		{
			//Test project resides in master db, as in prod
			FM = new ModelHelper(() => MainSession, () => CurrentUser, TestProject);
			//and test project stored in db with 3 demo projects (see TestDataService), that will be test
			//project code restriction correctness in queries
			M = new ModelHelper(() => ProjectSession("hd"), () => CurrentUser, TestProject);
		}

		protected virtual ISession Session
		{
			get { return ProjectSession(TestProject); }
		}

		protected ICrudDao ProjDao { get; private set; }
	}
}
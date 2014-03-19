using System;
using AGO.Core;
using AGO.Core.Tests;
using NHibernate;

namespace AGO.Tasks.Test
{
	public class AbstractTest : AbstractPersistenceTest<ModelHelper>
	{
		protected string TestProject { get; private set; }

		protected ModelHelper FPM { get; private set; }

		public override void FixtureSetUp()
		{
			TestProject = Guid.NewGuid().ToString().Replace("-", string.Empty);
			
			base.FixtureSetUp();

			SetupTestProject();

			ProjDao = DaoFactory.CreateProjectCrudDao(TestProject);
		}

		public override void FixtureTearDown()
		{
			if (FPM != null)
				FPM.DropCreated();
			base.FixtureTearDown();
		}

		public override void TearDown()
		{
			M.DeleteProjectActivity(TestProject, Session);
			Session.Flush();
			base.TearDown();
		}

		protected virtual void SetupTestProject()
		{
			var admin = LoginAdmin();

			//Test project resides in master db, as in prod
			FM = new ModelHelper(() => MainSession, () => Session, () => CurrentUser, TestProject);
			var proj = FM.Project(TestProject);
			proj.ConnectionString = ProjectSession("hd").Connection.ConnectionString;//hd project stored in db with personal and crm projects
			MainSession.Update(proj);
			MainSession.Flush();
			//but static members for all tests resides in project db, so, introduce additional fixture-level helper
			FPM = new ModelHelper(() => Session, () => Session, () => CurrentUser, TestProject);
			FPM.Member(proj, admin, TaskProjectRoles.Manager);
			
			//test project data stored in db with 3 demo projects (see TestDataService), that will be test
			//project code restriction correctness in queries
			M = new ModelHelper(() => Session, () => Session, () => CurrentUser, TestProject);
		}

		protected override void CreateModelHelpers()
		{
			//moved to other place, because part of helpers dependen from test project
		}

		/// <summary>
		/// Session to database of current <see cref="TestProject"/>
		/// </summary>
		protected virtual ISession Session
		{
			get { return ProjectSession(TestProject); }
		}

		protected ICrudDao ProjDao { get; private set; }
	}
}
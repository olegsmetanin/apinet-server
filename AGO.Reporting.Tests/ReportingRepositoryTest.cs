using System;
using System.Text;
using AGO.Core.Model.Projects;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;
using NUnit.Framework;

namespace AGO.Reporting.Tests
{
	public class ReportingRepositoryTest: AbstractReportingTest
	{
		private IReportingRepository repo;
		private ProjectModel project;
		private ModelHelper MM;

		public override void SetUp()
		{
			base.SetUp();
			repo = IocContainer.GetInstance<IReportingRepository>();
			var code = Guid.NewGuid().ToString().Replace("-", string.Empty);
			project = M.Project(code);
			//because master db does not have report tables (moved to proj db's), patch project cs to one of out test databases
			var dbcfg = DbConfiguratorFactory.CreateConfigurator("PostgreSQL", MasterConnectionString);
			project.ConnectionString = dbcfg.MakeConnectionString(null, "ago_apinet_others",
				MainSession.Connection.ConnectionString);
			MainSession.Update(project);
			MainSession.Flush();

			MM = new ModelHelper(() => ProjectSession(code), () => CurrentUser);
		}

		public override void TearDown()
		{
			project = null;
			repo = null;
			MM.DropCreated();
			MM = null;
			base.TearDown();
		}

		[Test]
		public void GetTask()
		{
			var tpl = MM.Template(project.ProjectCode, "my template", Encoding.UTF8.GetBytes("aaa bbb"));
			var setting = MM.Setting(project.ProjectCode, "my setting", tpl.Id);
			IReportTask task = MM.Task(project.ProjectCode, "my task", setting.Id);

			task = repo.GetTask(ProjectSession(project.ProjectCode), task.Id);

			Assert.IsNotNull(task);
			Assert.AreEqual(project.ProjectCode, task.ProjectCode);
			Assert.AreEqual("NUnit my task", task.Name);
			Assert.AreEqual(setting.Id, task.Setting.Id);
		}
	}
}
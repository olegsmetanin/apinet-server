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

		public override void SetUp()
		{
			base.SetUp();
			repo = IocContainer.GetInstance<IReportingRepository>();
			var code = Guid.NewGuid().ToString().Replace("-", string.Empty);
			project = M.Project(code);
		}

		public override void TearDown()
		{
			project = null;
			repo = null;
			base.TearDown();
		}

		[Test]
		public void GetTask()
		{
			var tpl = M.Template(project.ProjectCode, "my template", Encoding.UTF8.GetBytes("aaa bbb"));
			var setting = M.Setting(project.ProjectCode, "my setting", tpl.Id);
			IReportTask task = M.Task(project.ProjectCode, "my task", setting.Id);

			task = repo.GetTask(ProjectSession(project.ProjectCode), task.Id);

			Assert.IsNotNull(task);
			Assert.AreEqual(project.ProjectCode, task.ProjectCode);
			Assert.AreEqual("NUnit my task", task.Name);
			Assert.AreEqual(setting.Id, task.Setting.Id);
		}
	}
}
using System;
using System.Reflection;
using System.Text;
using AGO.Core;
using AGO.Core.Model.Reporting;
using AGO.Reporting.Common;
using AGO.Tasks.Reports;
using NHibernate;

namespace AGO.Tasks
{
	/// <summary>
	/// Report of tasks module
	/// </summary>
	internal sealed class TasksReports
	{
		internal void PopulateReports(ISession session, string project)
		{
			if (session == null)
				throw new ArgumentNullException("session");
			if (project.IsNullOrWhiteSpace())
				throw new ArgumentNullException("project");


			const string csvSimpleTemplate = "<range_data>\"{$num$}\",\"{$type$}\",\"{$executors$}\"</range_data>";
			const string csvDetailedTemplate = "<range_data>\"{$num$}\",\"{$type$}\",\"{$content$}\",\"{$dueDate$}\",\"{$executors$}\"</range_data>";
			byte[] xltSimpleTemplate;
			byte[] xltDetailedTemplate;
			byte[] xltDetailedWithParamsTemplate;
			using (var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream("AGO.Tasks.Reports.TaskList.xlt"))
			{
				System.Diagnostics.Debug.Assert(rs != null, "No report template resource in assembly");
				xltSimpleTemplate = new byte[rs.Length];
				rs.Read(xltSimpleTemplate, 0, xltSimpleTemplate.Length);
			}
			using (var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream("AGO.Tasks.Reports.DetailedTaskList.xlt"))
			{
				System.Diagnostics.Debug.Assert(rs != null, "No report template resource in assembly");
				xltDetailedTemplate = new byte[rs.Length];
				rs.Read(xltDetailedTemplate, 0, xltDetailedTemplate.Length);
			}
			using (var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream("AGO.Tasks.Reports.DetailedTaskListWithParams.xlt"))
			{
				System.Diagnostics.Debug.Assert(rs != null, "No report template resource in assembly");
				xltDetailedWithParamsTemplate = new byte[rs.Length];
				rs.Read(xltDetailedWithParamsTemplate, 0, xltDetailedWithParamsTemplate.Length);
			}

			var st = new ReportTemplateModel
			{
				CreationTime = DateTime.UtcNow,
				LastChange = DateTime.UtcNow,
				ProjectCode = project,
				Name = "TaskList.csv",
				Content = Encoding.UTF8.GetBytes(csvSimpleTemplate)
			};
			var dt = new ReportTemplateModel
			{
				CreationTime = DateTime.UtcNow,
				LastChange = DateTime.UtcNow,
				ProjectCode = project,
				Name = "DetailedTaskList.csv",
				Content = Encoding.UTF8.GetBytes(csvDetailedTemplate)
			};
			var xst = new ReportTemplateModel
			{
				CreationTime = DateTime.UtcNow,
				LastChange = DateTime.UtcNow,
				ProjectCode = project,
				Name = "TaskList.xlt",
				Content = xltSimpleTemplate
			};
			var xdt = new ReportTemplateModel
			{
				CreationTime = DateTime.UtcNow,
				LastChange = DateTime.UtcNow,
				ProjectCode = project,
				Name = "DetailedTaskList.xlt",
				Content = xltDetailedTemplate
			};
			var xdpt = new ReportTemplateModel
			{
				CreationTime = DateTime.UtcNow,
				LastChange = DateTime.UtcNow,
				ProjectCode = project,
				Name = "DetailedTaskListWithParams.xlt",
				Content = xltDetailedWithParamsTemplate
			};
			session.Save(st);
			session.Save(dt);
			session.Save(xst);
			session.Save(xdt);
			session.Save(xdpt);

			var ss = new ReportSettingModel
			{
				CreationTime = DateTime.UtcNow,
				ProjectCode = project,
				Name = "Task list (csv)",
				TypeCode = "task-list",
				DataGeneratorType = typeof(SimpleTaskListDataGenerator).AssemblyQualifiedName,
				GeneratorType = GeneratorType.CvsGenerator,
				ReportParameterType = typeof(TaskListReportParameters).AssemblyQualifiedName,
				ReportTemplate = st
			};
			var xss = new ReportSettingModel
			{
				CreationTime = DateTime.UtcNow,
				ProjectCode = project,
				Name = "Task list (MS Excel)",
				TypeCode = "task-list",
				DataGeneratorType = typeof(SimpleTaskListDataGenerator).AssemblyQualifiedName,
				GeneratorType = GeneratorType.XlsSyncFusionGenerator,
				ReportParameterType = typeof(TaskListReportParameters).AssemblyQualifiedName,
				ReportTemplate = xst
			};
			var ds = new ReportSettingModel
			{
				CreationTime = DateTime.UtcNow,
				ProjectCode = project,
				Name = "Detailed task list (csv)",
				TypeCode = "task-list",
				DataGeneratorType = typeof(DetailedTaskListDataGenerator).AssemblyQualifiedName,
				GeneratorType = GeneratorType.CvsGenerator,
				ReportParameterType = typeof(TaskListReportParameters).AssemblyQualifiedName,
				ReportTemplate = dt
			};
			var xds = new ReportSettingModel
			{
				CreationTime = DateTime.UtcNow,
				ProjectCode = project,
				Name = "Detailed task list (MS Excel)",
				TypeCode = "task-list",
				DataGeneratorType = typeof(DetailedTaskListDataGenerator).AssemblyQualifiedName,
				GeneratorType = GeneratorType.XlsSyncFusionGenerator,
				ReportParameterType = typeof(TaskListReportParameters).AssemblyQualifiedName,
				ReportTemplate = xdt
			};
			var xdps = new ReportSettingModel
			{
				CreationTime = DateTime.UtcNow,
				ProjectCode = project,
				Name = "Detailed task list with user props (MS Excel)",
				TypeCode = "task-list",
				DataGeneratorType = typeof(DetailedTaskListWithCustomPropsDataGenerator).AssemblyQualifiedName,
				GeneratorType = GeneratorType.XlsSyncFusionGenerator,
				ReportParameterType = typeof(TaskListReportParameters).AssemblyQualifiedName,
				ReportTemplate = xdpt
			};
			session.Save(ss);
			session.Save(xss);
			session.Save(ds);
			session.Save(xds);
			session.Save(xdps);

			var fake = new ReportSettingModel
			{
				CreationTime = DateTime.UtcNow,
				ProjectCode = project,
				Name = "Fake long running report (csv)",
				TypeCode = "task-list",
				DataGeneratorType = typeof(FakeLongRunningDataGenerator).AssemblyQualifiedName,
				GeneratorType = GeneratorType.CvsGenerator,
				ReportParameterType = typeof(TaskListReportParameters).AssemblyQualifiedName,
				ReportTemplate = st
			};
			session.Save(fake);
		}
	}
}
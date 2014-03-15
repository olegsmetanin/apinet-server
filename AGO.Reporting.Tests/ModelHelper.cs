using System;
using System.IO;
using AGO.Core.Model.Reporting;
using AGO.Core.Model.Security;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;
using NHibernate;

namespace AGO.Reporting.Tests
{
	public class ModelHelper: Core.Tests.ModelHelper
	{
		public ModelHelper(Func<ISession> session, Func<UserModel> currentUser): base(session, currentUser)
		{
		}

		public ReportTemplateModel Template(string project, string name, Stream content)
		{
			var buffer = new byte[content.Length];
			new BinaryReader(content).Read(buffer, 0, buffer.Length);
			return Template(project, name, buffer);
		}

		public ReportTemplateModel Template(string project, string name, byte[] template)
		{
			return Track(() =>
			{
				var tpl = new ReportTemplateModel
				{
					ProjectCode = project,
					Name = "NUnit " + name,
					Content = template,
					CreationTime = DateTime.Now,
					LastChange = DateTime.Now
				};
				Session().Save(tpl);
				Session().Flush();

				return tpl;
			});
		}

		public ReportSettingModel Setting(string project, string name, Guid templateId, 
			GeneratorType type = GeneratorType.CvsGenerator, string datagen = "fake", string param = "fake")
		{
			return Track(() =>
			{
				var setting = new ReportSettingModel
				{
					ProjectCode = project,
					Name = "NUnit " + name,
					TypeCode = "NUnit",
					GeneratorType = type,
					DataGeneratorType = datagen,
					ReportParameterType = param,
					ReportTemplate = Session().Get<ReportTemplateModel>(templateId)
				};
				Session().Save(setting);
				Session().Flush();

				return setting;
			});
		}

		public ReportTaskModel Task(string project, string name, Guid settingId, string param = null)
		{
			return Track(() =>
			{
				var setting = Session().Get<ReportSettingModel>(settingId);
				var task = new ReportTaskModel
				{
					ProjectCode = project,
					Culture = "en",
					Name = "NUnit " + name,
					ReportSetting = setting,
					Parameters = param,
					State = ReportTaskState.NotStarted,
					Creator = CurrentUser(),
					CreationTime = DateTime.UtcNow
				};
				Session().Save(task);
				Session().Flush();

				return task;
			});
		}
	}
}
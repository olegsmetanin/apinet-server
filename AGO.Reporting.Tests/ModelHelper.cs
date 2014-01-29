using System;
using System.IO;
using AGO.Core.Model.Reporting;
using AGO.Core.Model.Security;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;
using NHibernate;

namespace AGO.Reporting.Tests
{
	public class ModelHelper
	{
		private readonly Func<ISession> session;
		private readonly Func<UserModel> currentUser;

		public ModelHelper(Func<ISession> session, Func<UserModel> currentUser)
		{
			this.session = session;
			this.currentUser = currentUser;
		}

		public ReportTemplateModel Template(string name, Stream content)
		{
			var buffer = new byte[content.Length];
			new BinaryReader(content).Read(buffer, 0, buffer.Length);
			return Template(name, buffer);
		}

		public ReportTemplateModel Template(string name, byte[] template)
		{
			var tpl = new ReportTemplateModel
			          	{
			          		Name = "NUnit " + name,
			          		Content = template,
			          		CreationTime = DateTime.Now,
			          		LastChange = DateTime.Now
			          	};
			session().Save(tpl);
			session().Flush();

			return tpl;
		}

		public ReportSettingModel Setting(string name, Guid templateId, 
			GeneratorType type = GeneratorType.CvsGenerator, string datagen = "fake", string param = "fake")
		{
			var setting = new ReportSettingModel
			              	{
								Name = "NUnit " + name,
								TypeCode = "NUnit",
			              		GeneratorType = type,
			              		DataGeneratorType = datagen,
			              		ReportParameterType = param,
			              		ReportTemplate = session().Get<ReportTemplateModel>(templateId)
			              	};
			session().Save(setting);
			session().Flush();

			return setting;
		}

		public ReportTaskModel Task(string name, Guid settingId, string param = null)
		{
			var setting = session().Get<ReportSettingModel>(settingId);
			var task = new ReportTaskModel
			           	{
			           		Name = "NUnit " + name,
			           		ReportSetting = setting,
			           		Parameters = param,
			           		State = ReportTaskState.NotStarted,
							Creator = currentUser(),
							CreationTime = DateTime.UtcNow
			           	};
			session().Save(task);
			session().Flush();

			return task;
		}
	}
}
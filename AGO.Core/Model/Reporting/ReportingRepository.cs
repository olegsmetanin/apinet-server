using System;
using System.Collections.Generic;
using AGO.Core.Localization;
using AGO.Core.Model.Projects;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;

namespace AGO.Core.Model.Reporting
{
	/// <summary>
	/// Репозиторий для доступа к данным сервиса отчетов
	/// </summary>
	public class ReportingRepository: AbstractDao, IReportingRepository
	{
		private readonly ILocalizationService ls;

		public ReportingRepository(ISessionProvider sessionProvider, ILocalizationService ls) : base(sessionProvider)
		{
			if (ls == null)
				throw new ArgumentNullException("ls");

			this.ls = ls;
		}

		public IEnumerable<IReportingServiceDescriptor> GetAllDescriptors()
		{
			return CurrentSession.QueryOver<ReportingServiceDescriptorModel>().List<IReportingServiceDescriptor>();
		}

		public IReportingServiceDescriptor GetDescriptor(string name)
		{
			if (name.IsNullOrWhiteSpace())
				throw new ArgumentNullException("name");

			var descriptor = CurrentSession.QueryOver<ReportingServiceDescriptorModel>()
				.Where(m => m.Name == name).SingleOrDefault();
			if (descriptor == null)
				throw new NoSuchEntityException();
			return descriptor;
		}

		public IReportTask GetTask(Guid taskId)
		{
			return CurrentSession.Load<ReportTaskModel>(taskId);
		}

		public IReportTemplate GetTemplate(Guid templateId)
		{
			return CurrentSession.Load<ReportTemplateModel>(templateId);
		}

		public object GetTaskAsDTO(Guid taskId)
		{
			var task = CurrentSession.Load<ReportTaskModel>(taskId);
			var proj = CurrentSession.QueryOver<ProjectModel>().Where(m => m.ProjectCode == task.Project).SingleOrDefault();
			return ReportTaskDTO.FromTask(task, ls, proj != null ? proj.Name : null);
		}
	}
}
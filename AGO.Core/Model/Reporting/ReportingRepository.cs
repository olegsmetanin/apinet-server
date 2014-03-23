using System;
using AGO.Core.Localization;
using AGO.Core.Model.Projects;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;
using NHibernate;

namespace AGO.Core.Model.Reporting
{
	/// <summary>
	/// Репозиторий для доступа к данным сервиса отчетов
	/// </summary>
	public class ReportingRepository: IReportingRepository
	{
		private readonly ILocalizationService ls;

		public ReportingRepository(ILocalizationService ls)
		{
			if (ls == null)
				throw new ArgumentNullException("ls");

			this.ls = ls;
		}

		public IReportTask GetTask(ISession session, Guid taskId)
		{
			return session.Load<ReportTaskModel>(taskId);
		}

		public IReportTemplate GetTemplate(ISession session, Guid templateId)
		{
			return session.Load<ReportTemplateModel>(templateId);
		}

		public object GetTaskAsDTO(ISession mainDbSession, ISession projectDbSession, Guid taskId)
		{
			var task = projectDbSession.Load<ReportTaskModel>(taskId);
			var proj = mainDbSession.QueryOver<ProjectModel>().Where(m => m.ProjectCode == task.ProjectCode).SingleOrDefault();
			return ReportTaskDTO.FromTask(task, ls, proj != null ? proj.Name : null);
		}
	}
}

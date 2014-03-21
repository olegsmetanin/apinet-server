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
			var proj = GetProject(mainDbSession, task.ProjectCode);
			return ReportTaskDTO.FromTask(task, ls, proj != null ? proj.Name : null);
		}

		public void ArchiveReport(ISession mainDbSession, IReportTask report)
		{
			var proj = GetProject(mainDbSession, report.ProjectCode);
			var archiveRecord = new ReportArchiveRecordModel
			{
				CreationTime = report.CompletedAt.GetValueOrDefault(DateTime.UtcNow),
				ReportTaskId = report.Id,
				ProjectCode = proj.ProjectCode,
				ProjectName = proj.Name,
				ProjectType = proj.Type.Name,
				Name = report.ResultName,
				SettingsName = report.Setting.Name,
				UserId = report.AuthorId
			};
			mainDbSession.Save(archiveRecord);
		}
		
		private static ProjectModel GetProject(ISession mainDbSession, string project)
		{
			return mainDbSession.QueryOver<ProjectModel>().Where(m => m.ProjectCode == project).SingleOrDefault();
		}
	}
}

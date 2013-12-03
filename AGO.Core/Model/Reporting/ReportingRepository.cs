using System;
using System.Collections.Generic;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;

namespace AGO.Core.Model.Reporting
{
	/// <summary>
	/// Репозиторий для доступа к данным сервиса отчетов
	/// </summary>
	public class ReportingRepository: AbstractDao, IReportingRepository
	{
		public ReportingRepository(ISessionProvider sessionProvider) : base(sessionProvider)
		{
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

		public IReportTask GetTask(Guid taskid)
		{
			return CurrentSession.Load<ReportTaskModel>(taskid);
		}

		public IReportTemplate GetTemplate(Guid templateId)
		{
			return CurrentSession.Load<ReportTemplateModel>(templateId);
		}
	}
}
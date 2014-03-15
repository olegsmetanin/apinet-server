using System;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;
using SimpleInjector;

namespace AGO.Reporting.Service.Workers
{
	public class CustomReportWorker: AbstractReportWorker
	{
		public CustomReportWorker(string project, Guid taskId, Container di, TemplateResolver resolver) : base(project, taskId, di, resolver)
		{
		}

		public override void Prepare(IReportTask task)
		{
			throw new System.NotImplementedException();
		}

		protected override IReportGeneratorResult InternalStart()
		{
			throw new System.NotImplementedException();
		}

		protected override void InternalTrackProgress(IReportTask task)
		{
			throw new System.NotImplementedException();
		}
	}
}
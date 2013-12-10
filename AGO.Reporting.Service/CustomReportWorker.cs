using System;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;
using SimpleInjector;

namespace AGO.Reporting.Service
{
	public class CustomReportWorker: AbstractReportWorker
	{
		public CustomReportWorker(Guid taskId, Container di, TemplateResolver resolver) : base(taskId, di, resolver)
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
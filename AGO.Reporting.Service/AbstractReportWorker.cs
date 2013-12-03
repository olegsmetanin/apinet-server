using System;

namespace AGO.Reporting.Service
{
	/// <summary>
	/// Базовый класс для worker-ов - генераторов отчетов.
	/// </summary>
	public abstract class AbstractReportWorker
	{
		public Guid TaskId { get; set; }

		public TemplateResolver TemplateResolver { get; set; }

		public object Parameters { get; set; }

		public bool Finished { get; protected set; }

		public TimeSpan Timeout { get; set; }

		public abstract void Start();
	}
}
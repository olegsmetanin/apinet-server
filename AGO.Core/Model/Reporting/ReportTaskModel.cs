using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Lob;
using AGO.Reporting.Common.Model;

namespace AGO.Core.Model.Reporting
{
	/// <summary>
	/// Модель задачи на создание отчета
	/// </summary>
	[MetadataExclude]
	public class ReportTaskModel: CoreModel<Guid>, IReportTask
	{
		#region Persistent

		[NotEmpty, NotLonger(250)]
		public virtual string Name { get; set; }

		[NotNull]
		public virtual ReportSettingModel ReportSetting { get; set; }
		[ReadOnlyProperty]
		public virtual Guid? ReportSettingId { get; set; }

		[NotNull]
		public virtual ReportingServiceDescriptorModel ReportingService { get; set; }
		[ReadOnlyProperty]
		public virtual Guid? ReportingServiceId { get; set; }

		public virtual string Parameters { get; set; }

		public virtual ReportTaskState State { get; set; }

		[InRange(0, 100)]
		public virtual byte DateGenerationProgress { get; set; }

		[InRange(0, 100)]
		public virtual byte ReportGenerationProgress { get; set; }

		public virtual DateTime? StartedAt { get; set; }

		public virtual DateTime? CompletedAt { get; set; }

		public virtual string ErrorMsg { get; set; }

		public virtual string ErrorDetails { get; set; }

		public virtual ArrayBlob ResultContent { get; set; }

		[NotLonger(128)]
		public virtual string ResultContentType { get; set; }

		#endregion

		[NotMapped]
		public virtual IReportSetting Setting
		{
			get { return ReportSetting; }
		}

		[NotMapped]
		public virtual IReportingServiceDescriptor Service
		{
			get { return ReportingService; }
		}

		[NotMapped]
		public virtual byte[] Result
		{
			get { return ResultContent != null ? ResultContent.Data : null; } 
			set { ResultContent = value != null ? new ArrayBlob(value) : null; }
		}
	}
}
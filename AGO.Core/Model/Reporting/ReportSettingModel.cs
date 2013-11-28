using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;

namespace AGO.Core.Model.Reporting
{
	/// <summary>
	/// Модель настроек отчета
	/// </summary>
	[MetadataExclude]
	public class ReportSettingModel: CoreModel<Guid>, IReportSetting
	{
		#region Persistent

		[NotEmpty, UniqueProperty]
		public virtual string Name { get; set; }

		public virtual GeneratorType GeneratorType { get; set; }

		[NotEmpty, NotLonger(2048)]
		public virtual string DataGeneratorType { get; set; }

		[NotEmpty, NotLonger(2048)]
		public virtual string ReportParameterType { get; set; }

		[NotNull]
		public virtual ReportTemplateModel ReportTemplate { get; set; }
		[ReadOnlyProperty]
		public virtual Guid? ReportTemplateId { get; set; }

		#endregion

		[NotMapped]
		public virtual IReportTemplate Template
		{
			get { return ReportTemplate; }
		}
	}
}
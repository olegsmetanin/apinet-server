using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Reporting
{
	/// <summary>
	/// Модель настроек отчета
	/// </summary>
	public class ReportSettingModel: CoreModel<Guid>, IReportSetting
	{
		#region Persistent

		[NotEmpty, UniqueProperty, JsonProperty]
		public virtual string Name { get; set; }

		[NotEmpty, NotLonger(128), JsonProperty]
		public virtual string TypeCode { get; set; }

		[JsonProperty]
		public virtual GeneratorType GeneratorType { get; set; }

		[NotEmpty, NotLonger(2048), JsonProperty]
		public virtual string DataGeneratorType { get; set; }

		[NotEmpty, NotLonger(2048), JsonProperty]
		public virtual string ReportParameterType { get; set; }

		[NotNull, JsonProperty]
		public virtual ReportTemplateModel ReportTemplate { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? ReportTemplateId { get; set; }

		#endregion

		[NotMapped]
		public virtual IReportTemplate Template
		{
			get { return ReportTemplate; }
		}
	}
}
using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Lob;
using AGO.Reporting.Common.Model;

namespace AGO.Core.Model.Reporting
{
	/// <summary>
	/// Модель шаблона отчета
	/// </summary>
	[MetadataExclude]
	public class ReportTemplateModel: CoreModel<Guid>, IReportTemplate
	{
		#region Persistent

		[NotEmpty, NotLonger(250)]
		public virtual string Name { get; set; }

		[NotEmpty]
		public virtual ArrayBlob TemplateContent { get; set; }


		[NotEmpty]
		public virtual DateTime LastChange { get; set; }

		#endregion

		[NotMapped]
		public virtual byte[] Content
		{
			get { return TemplateContent != null ? TemplateContent.Data : null; }
			set { TemplateContent = value != null ? new ArrayBlob(value) : null; }
		}
	}
}
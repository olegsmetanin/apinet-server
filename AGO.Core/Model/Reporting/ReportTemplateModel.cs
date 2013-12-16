using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Lob;
using AGO.Reporting.Common.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Reporting
{
	/// <summary>
	/// Модель шаблона отчета
	/// </summary>
	public class ReportTemplateModel: CoreModel<Guid>, IReportTemplate
	{
		#region Persistent

		[NotEmpty, NotLonger(250), JsonProperty]
		public virtual string Name { get; set; }

		[NotEmpty, MetadataExclude]
		public virtual ArrayBlob TemplateContent { get; set; }

		[NotEmpty, Timestamp, JsonProperty]
		public virtual DateTime LastChange { get; set; }

		#endregion

		[NotMapped]
		public virtual byte[] Content
		{
			get { return TemplateContent != null ? TemplateContent.Data : null; }
			set { TemplateContent = value != null ? new ArrayBlob(value) : null; }
		}

		[NotMapped, JsonProperty]
		public virtual int Size
		{
			get { return Content != null ? Content.Length : 0; }
		}
	}
}
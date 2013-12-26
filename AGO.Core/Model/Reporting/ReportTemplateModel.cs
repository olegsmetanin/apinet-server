using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Reporting.Common.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Reporting
{
	/// <summary>
	/// Модель шаблона отчета
	/// </summary>
	[LazyLoad]
	public class ReportTemplateModel: CoreModel<Guid>, IReportTemplate
	{
		#region Persistent

		[NotEmpty, NotLonger(250), JsonProperty, UniqueProperty]
		public virtual string Name { get; set; }

		[NotEmpty, LazyLoad, MetadataExclude]
		public virtual byte[] Content { get; set; }

		[NotEmpty, Timestamp, JsonProperty]
		public virtual DateTime LastChange { get; set; }

		#endregion

		[NotMapped, JsonProperty]
		public virtual int Size
		{
			get { return Content != null ? Content.Length : 0; }
		}
	}
}
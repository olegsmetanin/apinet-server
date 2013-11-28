using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Reporting.Common.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Reporting
{
	/// <summary>
	/// Модель настроек экземпляра сервиса отчетов
	/// </summary>
	[MetadataExclude]
	public class ReportingServiceDescriptorModel: CoreModel<Guid>, IReportingServiceDescriptor
	{
		#region Persistent

		[NotLonger(450), JsonProperty, NotEmpty, UniqueProperty]
		public virtual string Name { get; set; }

		[NotLonger(1024), JsonProperty, NotEmpty, UniqueProperty]
		public virtual string EndPoint { get; set; }

		[JsonProperty]
		public virtual bool LongRunning { get; set; }

		#endregion

		public override string ToString()
		{
			return Name + " (" + EndPoint + ")";
		}
	}
}
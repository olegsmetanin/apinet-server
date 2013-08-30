using System;
using System.ComponentModel;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Model.Security;
using AGO.DocManagement.Model.Dictionary.Documents;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.DocManagement.Model.Documents
{
	public class DocumentStatusHistoryModel : SecureModel<Guid>
	{
		#region Persistent

		[DisplayName("Дата начала"), JsonProperty, NotNull]
		public virtual DateTime? StartDate { get; set; }

		[DisplayName("Дата конца"), JsonProperty]
		public virtual DateTime? EndDate { get; set; }

		[DisplayName("Документ"), JsonProperty, NotNull]
		public virtual DocumentModel Document { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? DocumentId { get; set; }

		[DisplayName("Статус"), JsonProperty, NotNull]
		public virtual DocumentStatusModel Status { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? StatusId { get; set; }

		#endregion
	}
}

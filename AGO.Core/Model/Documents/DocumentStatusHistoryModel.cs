using System;
using System.ComponentModel;
using AGO.Core.Model.Dictionary.Documents;
using AGO.Core.Model.Security;
using AGO.Hibernate.Attributes.Constraints;
using AGO.Hibernate.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Documents
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
		[ReadOnlyProperty]
		public virtual Guid? DocumentId { get; set; }

		[DisplayName("Статус"), JsonProperty, NotNull]
		public virtual DocumentStatusModel Status { get; set; }
		[ReadOnlyProperty]
		public virtual Guid? StatusId { get; set; }

		#endregion
	}
}

using System;
using System.ComponentModel;
using AGO.Docstore.Model.Dictionary;
using AGO.Docstore.Model.Security;
using AGO.Hibernate.Attributes.Constraints;
using Newtonsoft.Json;

namespace AGO.Docstore.Model.Documents
{
	public class DocumentStatusHistoryModel : SecureModel<Guid>
	{
		#region Persistent

		[DisplayName("Дата начала"), JsonProperty, NotNull]
		public virtual DateTime? StartDate { get; set; }

		[DisplayName("Дата конца"), JsonProperty]
		public virtual DateTime? EndDate { get; set; }

		[DisplayName("Документ"), /*JsonProperty,*/ NotNull]
		public virtual DocumentModel Document { get; set; }

		[DisplayName("Статус"), /*JsonProperty,*/ NotNull]
		public virtual DocumentStatusModel Status { get; set; }

		#endregion
	}
}

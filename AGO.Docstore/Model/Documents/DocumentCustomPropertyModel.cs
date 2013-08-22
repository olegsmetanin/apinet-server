using System;
using System.ComponentModel;
using AGO.Docstore.Model.Dictionary;
using AGO.Hibernate.Attributes.Constraints;
using AGO.Hibernate.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Docstore.Model.Documents
{
	public class DocumentCustomPropertyModel : CustomPropertyInstanceModel
	{
		#region Persistent

		[DisplayName("Документ"), JsonProperty, NotNull]
		public virtual DocumentModel Document { get; set; }
		[ReadOnlyProperty]
		public virtual Guid? DocumentId { get; set; }

		#endregion
	}
}

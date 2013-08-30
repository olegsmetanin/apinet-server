using System;
using System.ComponentModel;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Dictionary;
using Newtonsoft.Json;

namespace AGO.DocManagement.Model.Documents
{
	public class DocumentCustomPropertyModel : CustomPropertyInstanceModel
	{
		#region Persistent

		[DisplayName("Документ"), JsonProperty, NotNull]
		public virtual DocumentModel Document { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? DocumentId { get; set; }

		#endregion
	}
}

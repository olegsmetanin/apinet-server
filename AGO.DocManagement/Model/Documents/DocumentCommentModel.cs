using System;
using System.ComponentModel;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Model.Security;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.DocManagement.Model.Documents
{
	public class DocumentCommentModel : SecureModel<Guid>
	{
		#region Persistent

		[DisplayName("Автор - внешний пользователь"), JsonProperty, NotLonger(128)]
		public virtual string ExternalAuthor { get; set; }

		[DisplayName("Текст"), JsonProperty, NotEmpty]
		public virtual string Text { get; set; }

		[DisplayName("Документ"), JsonProperty, NotNull]
		public virtual DocumentModel Document { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? DocumentId { get; set; }

		#endregion
	}
}

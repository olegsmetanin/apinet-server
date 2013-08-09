using System;
using System.ComponentModel;
using AGO.Docstore.Model.Security;
using AGO.Hibernate.Attributes.Constraints;
using Newtonsoft.Json;

namespace AGO.Docstore.Model.Documents
{
	public class DocumentCommentModel : SecureModel<Guid>
	{
		#region Persistent

		[DisplayName("Автор - внешний пользователь"), JsonProperty, NotLonger(128)]
		public virtual string ExternalAuthor { get; set; }

		[DisplayName("Текст"), JsonProperty, NotEmpty]
		public virtual string Text { get; set; }

		[DisplayName("Документ"), /*JsonProperty,*/ NotNull]
		public virtual DocumentModel Document { get; set; }

		#endregion
	}
}

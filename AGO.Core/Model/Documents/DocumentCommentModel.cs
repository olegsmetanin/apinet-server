using System;
using System.ComponentModel;
using AGO.Core.Model.Security;
using AGO.Hibernate.Attributes.Constraints;
using AGO.Hibernate.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Documents
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
		[ReadOnlyProperty]
		public virtual Guid? DocumentId { get; set; }

		#endregion
	}
}

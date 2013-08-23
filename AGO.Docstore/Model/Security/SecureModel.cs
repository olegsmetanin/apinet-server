using System;
using System.ComponentModel;
using AGO.Hibernate.Attributes.Constraints;
using AGO.Hibernate.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Docstore.Model.Security
{
	public abstract class SecureModel<TIdType> : DocstoreModel<TIdType>, ISecureModel
	{
		#region Persistent

		[DisplayName("Кто создал"), NotNull]
		public virtual UserModel Creator { get; set; }
		[ReadOnlyProperty]
		public virtual Guid? CreatorId { get; set; }

		[DisplayName("Когда последний раз редактировали"), JsonProperty]
		public virtual DateTime? LastChangeTime { get; set; }

		[DisplayName("Кто последний раз редактировал")]
		public virtual UserModel LastChanger { get; set; }
		[ReadOnlyProperty]
		public virtual Guid? LastChangerId { get; set; }
		
		#endregion
	}
}

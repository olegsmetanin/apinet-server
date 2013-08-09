using System;
using System.ComponentModel;
using AGO.Hibernate.Attributes.Constraints;
using Newtonsoft.Json;

namespace AGO.Docstore.Model.Security
{
	public abstract class SecureModel<TIdType> : DocstoreModel<TIdType>, ISecureModel
	{
		#region Persistent

		[DisplayName("Кто создал"), /*JsonProperty,*/ NotNull]
		public virtual UserModel Creator { get; set; }

		[DisplayName("Когда последний раз редактировали"), JsonProperty,]
		public virtual DateTime? LastChangeTime { get; set; }

		[DisplayName("Кто последний раз редактировал"), /*JsonProperty,*/]
		public virtual UserModel LastChanger { get; set; }
		
		#endregion
	}
}

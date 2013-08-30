using System;
using System.ComponentModel;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Security
{
	public abstract class SecureModel<TIdType> : DocstoreModel<TIdType>, ISecureModel
	{
		#region Persistent

		[DisplayName("Кто создал"), NotNull]
		public virtual UserModel Creator { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? CreatorId { get; set; }

		[DisplayName("Когда последний раз редактировали"), JsonProperty]
		public virtual DateTime? LastChangeTime { get; set; }

		[DisplayName("Кто последний раз редактировал")]
		public virtual UserModel LastChanger { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? LastChangerId { get; set; }
		
		#endregion
	}
}

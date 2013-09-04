using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Core.Model.Security;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Dictionary
{
	[TablePerSubclass("ModelType")]
	public class TagModel : SecureModel<Guid>, IHierarchicalDictionaryItemModel
	{
		#region Persistent

		[DisplayName("Код проекта"), JsonProperty, NotLonger(32)]
		public virtual string ProjectCode { get; set; }

		[DisplayName("Наименование"), JsonProperty, NotLonger(128), NotEmpty]
		public virtual string Name { get; set; }

		[DisplayName("Полное наименование"), JsonProperty, NotLonger(1024)]
		public virtual string FullName { get; set; }

		[DisplayName("Владелец")]
		public virtual UserModel Owner { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? OwnerId { get; set; }

		[DisplayName("Предшественник"), JsonProperty]
		public virtual TagModel Parent { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? ParentId { get; set; }

		[DisplayName("Последователи"), PersistentCollection]
		public virtual ISet<TagModel> Children { get { return _Children; } set { _Children = value; } }
		private ISet<TagModel> _Children = new HashSet<TagModel>();

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return Name;
		}

		#endregion
	}
}
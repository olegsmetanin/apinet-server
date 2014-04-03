using System;
using System.Collections.Generic;
using AGO.Core.Model.Security;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Dictionary
{
	public class CustomPropertyTypeModel : SecureProjectBoundModel<Guid>, IHierarchicalDictionaryItemModel<CustomPropertyTypeModel>
	{
		#region Persistent

		[JsonProperty, NotLonger(128), NotEmpty]
		public virtual string Name { get; set; }

		[JsonProperty, NotLonger(1024)]
		public virtual string FullName { get; set; }

		[JsonProperty, NotLonger(64)]
		public virtual string Format { get; set; }

		[JsonProperty]
		public virtual CustomPropertyValueType ValueType { get; set; }

		[JsonProperty]
		public virtual CustomPropertyTypeModel Parent { get; set; }
		[ReadOnlyProperty, MetadataExclude, JsonProperty]
		public virtual Guid? ParentId { get; set; }

		[PersistentCollection]
		public virtual ISet<CustomPropertyTypeModel> Children { get { return _Children; } set { _Children = value; } }
		private ISet<CustomPropertyTypeModel> _Children = new HashSet<CustomPropertyTypeModel>();

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return Name;
		}

		#endregion
	}
}

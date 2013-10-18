using System;
using System.Collections.Generic;
using AGO.Core.Model.Security;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Dictionary
{
	public class DepartmentModel : SecureProjectBoundModel<Guid>, IHierarchicalDictionaryItemModel
	{
		#region Persistent

		[JsonProperty, NotLonger(128), NotEmpty]
		public virtual string Name { get; set; }

		[JsonProperty, NotLonger(1024)]
		public virtual string FullName { get; set; }

		[JsonProperty]
		public virtual DepartmentModel Parent { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? ParentId { get; set; }

		[PersistentCollection]
		public virtual ISet<DepartmentModel> Children { get { return _Children; } set { _Children = value; } }
		private ISet<DepartmentModel> _Children = new HashSet<DepartmentModel>();

		[PersistentCollection]
		public virtual ISet<UserModel> Users { get { return _Users; } set { _Users = value; } }
		private ISet<UserModel> _Users = new HashSet<UserModel>();

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return Name;
		}

		#endregion
	}
}

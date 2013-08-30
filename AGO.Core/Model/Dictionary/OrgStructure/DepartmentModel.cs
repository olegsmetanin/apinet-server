using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Core.Model.Security;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Dictionary.OrgStructure
{
	public class DepartmentModel : SecureModel<Guid>, IHierarchicalDictionaryItemModel
	{
		#region Persistent

		[DisplayName("Код проекта"), JsonProperty, NotLonger(32), NotEmpty]
		public virtual string ProjectCode { get; set; }

		[DisplayName("Наименование"), JsonProperty, NotLonger(128), NotEmpty]
		public virtual string Name { get; set; }

		[DisplayName("Полное наименование"), JsonProperty, NotLonger(1024)]
		public virtual string FullName { get; set; }

		[DisplayName("Предшественник"), JsonProperty]
		public virtual DepartmentModel Parent { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? ParentId { get; set; }

		[DisplayName("Последователи"), PersistentCollection]
		public virtual ISet<DepartmentModel> Children { get { return _Children; } set { _Children = value; } }
		private ISet<DepartmentModel> _Children = new HashSet<DepartmentModel>();

		[DisplayName("Пользователи"), PersistentCollection]
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

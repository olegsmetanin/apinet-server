using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using Newtonsoft.Json;

namespace AGO.Core.Model.Example
{
	public class HierarchicalModel : CommonModel<Guid>
	{
		[DisplayName("Наименование"), NotEmpty, NotLonger(64), JsonProperty]
		public virtual string Name { get; set; }

		[DisplayName("Родительский узел")]
		public virtual HierarchicalModel Parent { get; set; }

		[DisplayName("Дочерние узлы"), PersistentCollection]
		public virtual ISet<HierarchicalModel> Children { get { return _Children; } set { _Children = value; } }
		private ISet<HierarchicalModel> _Children = new HashSet<HierarchicalModel>();

		public virtual ManyEndModel ManyEnd { get; set; }
	}
}
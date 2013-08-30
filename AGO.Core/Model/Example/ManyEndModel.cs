using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using Newtonsoft.Json;

namespace AGO.Core.Model.Example
{
	public class ManyEndModel : CommonModel<Guid>
	{
		[DisplayName("Наименование"), NotEmpty, NotLonger(64), JsonProperty]
		public virtual string Name { get; set; }

		[DisplayName("Коллекция OneEndModels"), PersistentCollection]
		public virtual ISet<OneEndModel> OneEndModels { get { return _OneEndModels; } set { _OneEndModels = value; } }
		private ISet<OneEndModel> _OneEndModels = new HashSet<OneEndModel>();

		[DisplayName("Коллекция PrimitiveModels"), PersistentCollection]
		public virtual ISet<PrimitiveModel> PrimitiveModels { get { return _PrimitiveModels; } set { _PrimitiveModels = value; } }
		private ISet<PrimitiveModel> _PrimitiveModels = new HashSet<PrimitiveModel>();

		[DisplayName("Коллекция HierarchicalModels"), PersistentCollection]
		public virtual ISet<HierarchicalModel> HierarchicalModels { get { return _HierarchicalModels; } set { _HierarchicalModels = value; } }
		private ISet<HierarchicalModel> _HierarchicalModels = new HashSet<HierarchicalModel>();
	}
}
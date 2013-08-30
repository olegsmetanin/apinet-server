using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using Newtonsoft.Json;

namespace AGO.Core.Model.Example
{
	public class ManyToMany2Model : CommonModel<Guid>
	{
		[DisplayName("Наименование"), NotEmpty, NotLonger(64), JsonProperty]
		public virtual string Name { get; set; }

		[DisplayName("Коллекция AssociatedModels"), PersistentCollection]
		public virtual ISet<ManyToMany1Model> AssociatedModels { get { return _AssociatedModels; } set { _AssociatedModels = value; } }
		private ISet<ManyToMany1Model> _AssociatedModels = new HashSet<ManyToMany1Model>();
	}
}
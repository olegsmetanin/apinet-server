using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Hibernate.Attributes.Constraints;
using AGO.Hibernate.Attributes.Mapping;
using Newtonsoft.Json;

namespace AGO.Hibernate.Model.Example
{
	public class ManyToMany1Model : CommonModel<Guid>
	{
		[DisplayName("Наименование"), NotEmpty, NotLonger(64), JsonProperty]
		public virtual string Name { get; set; }

		[DisplayName("Коллекция AssociatedModels"), PersistentCollection(Inverse = false)]
		public virtual ISet<ManyToMany2Model> AssociatedModels { get { return _AssociatedModels; } set { _AssociatedModels = value; } }
		private ISet<ManyToMany2Model> _AssociatedModels = new HashSet<ManyToMany2Model>();
	}
}
using System;
using System.ComponentModel;
using AGO.Hibernate.Attributes.Constraints;
using Newtonsoft.Json;

namespace AGO.Hibernate.Model.Example
{
	public class OneEndModel : CommonModel<Guid>
	{
		[DisplayName("Наименование"), NotEmpty, NotLonger(64), JsonProperty]
		public virtual string Name { get; set; }

		[DisplayName("Ссылка на ManyEndModel"), NotNull]
		public virtual ManyEndModel ManyEnd { get; set; }
	}
}
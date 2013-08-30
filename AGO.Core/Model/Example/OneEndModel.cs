using System;
using System.ComponentModel;
using AGO.Core.Attributes.Constraints;
using Newtonsoft.Json;

namespace AGO.Core.Model.Example
{
	public class OneEndModel : CommonModel<Guid>
	{
		[DisplayName("Наименование"), NotEmpty, NotLonger(64), JsonProperty]
		public virtual string Name { get; set; }

		[DisplayName("Ссылка на ManyEndModel"), NotNull]
		public virtual ManyEndModel ManyEnd { get; set; }
	}
}
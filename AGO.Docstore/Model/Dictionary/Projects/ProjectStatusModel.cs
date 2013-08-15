using System;
using System.ComponentModel;
using AGO.Docstore.Model.Security;
using AGO.Hibernate.Attributes.Constraints;
using Newtonsoft.Json;

namespace AGO.Docstore.Model.Dictionary.Projects
{
	public class ProjectStatusModel : SecureModel<Guid>, IDictionaryItemModel
	{
		#region Persistent

		[DisplayName("Код проекта"), JsonProperty, NotLonger(32), NotEmpty]
		public virtual string ProjectCode { get; set; }

		[DisplayName("Наименование"), NotEmpty, NotLonger(64), JsonProperty]
		public virtual string Name { get; set; }

		[DisplayName("Описание"), NotLonger(512), JsonProperty]
		public virtual new string Description { get; set; }

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return Name;
		}

		#endregion
	}
}

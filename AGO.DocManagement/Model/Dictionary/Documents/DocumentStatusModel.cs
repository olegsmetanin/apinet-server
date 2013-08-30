using System;
using System.ComponentModel;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Security;
using AGO.Core.Attributes.Constraints;
using Newtonsoft.Json;

namespace AGO.DocManagement.Model.Dictionary.Documents
{
	public class DocumentStatusModel : SecureModel<Guid>, IDictionaryItemModel
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

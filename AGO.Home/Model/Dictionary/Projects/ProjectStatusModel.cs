using System;
using System.ComponentModel;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Security;
using AGO.Core.Attributes.Constraints;
using Newtonsoft.Json;

namespace AGO.Home.Model.Dictionary.Projects
{
	public class ProjectStatusModel : SecureProjectBoundModel<Guid>, IDictionaryItemModel
	{
		#region Persistent

		[DisplayName("Наименование"), UniqueProperty, NotEmpty, NotLonger(64), JsonProperty]
		public virtual string Name { get; set; }

		[DisplayName("Описание"), NotLonger(512), JsonProperty]
		public virtual new string Description { get; set; }

		[DisplayName("Начальный"), JsonProperty]
		public virtual bool IsInitial { get; set; }

		[DisplayName("Конечный"), JsonProperty]
		public virtual bool IsFinal { get; set; }

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return Name;
		}

		#endregion
	}
}

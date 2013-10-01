using System;
using System.ComponentModel;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Security;
using Newtonsoft.Json;

namespace AGO.Tasks.Model.Dictionary
{
	/// <summary>
	/// Тип задачи
	/// </summary>
	public class TaskTypeModel: SecureProjectBoundModel<Guid>, IDictionaryItemModel
	{
		/// <summary>
		/// Наименование
		/// </summary>
		[DisplayName("Наименование"), JsonProperty, UniqueProperty, NotEmpty, NotLonger(256)]
		public virtual string Name { get; set; }
	}
}
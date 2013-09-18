using System;
using System.ComponentModel;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Dictionary;
using Newtonsoft.Json;

namespace AGO.Tasks.Model.Task
{
	/// <summary>
	/// Пользователькое свойство задачи
	/// </summary>
	public class TaskCustomPropertyModel: CustomPropertyInstanceModel
	{
		/// <summary>
		/// Задача - владелец свойства
		/// </summary>
		[DisplayName("Задача"), JsonProperty, NotNull]
		public virtual TaskModel Task { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? TaskId { get; set; }
	}
}
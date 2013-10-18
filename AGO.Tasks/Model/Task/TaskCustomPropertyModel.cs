using System;
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
	public class TaskCustomPropertyModel : CustomPropertyInstanceModel, ITasksModel
	{
		/// <summary>
		/// Задача - владелец свойства
		/// </summary>
		[JsonProperty, NotNull]
		public virtual TaskModel Task { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? TaskId { get; set; }
	}
}
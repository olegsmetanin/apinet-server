using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Security;
using AGO.Tasks.Model.Dictionary;
using Newtonsoft.Json;

namespace AGO.Tasks.Model.Task
{
	/// <summary>
	/// Модель связи между задачей и тегом задачи
	/// </summary>
	public class TaskToTagModel: SecureModel<Guid>, ITasksModel
	{
		[JsonProperty, NotNull]
		public virtual TaskModel Task { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? TaskId { get; set; }

		[JsonProperty, NotNull]
		public virtual TaskTagModel Tag { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? TagId { get; set; }
	}
}
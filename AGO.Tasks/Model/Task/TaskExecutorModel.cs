using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Security;
using AGO.Core.Model.Projects;

namespace AGO.Tasks.Model.Task
{
	/// <summary>
	/// Исполнитель задачи
	/// </summary>
	public class TaskExecutorModel : SecureModel<Guid>, ITasksModel
	{
		/// <summary>
		/// Задача, на которую назначен исполнитель
		/// </summary>
		[NotNull]
		public virtual TaskModel Task { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? TaskId { get; set; }

		/// <summary>
		/// Исполнитель (участник проекта)
		/// </summary>
		[NotNull]
		public virtual ProjectParticipantModel Executor { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? ExecutorId { get; set; }
	}
}
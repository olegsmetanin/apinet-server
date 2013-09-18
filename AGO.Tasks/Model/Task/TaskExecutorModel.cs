using System;
using System.ComponentModel;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Security;
using AGO.Home.Model.Projects;

namespace AGO.Tasks.Model.Task
{
	/// <summary>
	/// Исполнитель задачи
	/// </summary>
	[Table(SchemaName = "Tasks")]
	public class TaskExecutorModel: SecureModel<Guid>
	{
		/// <summary>
		/// Задача, на которую назначен исполнитель
		/// </summary>
		[DisplayName("Задача"), NotNull]
		public virtual TaskModel Task { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? TaskId { get; set; }

		/// <summary>
		/// Исполнитель (участник проекта)
		/// </summary>
		[DisplayName("Исполнитель"), NotNull]
		public virtual ProjectParticipantModel Executor { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? ExecutorId { get; set; }
	}
}
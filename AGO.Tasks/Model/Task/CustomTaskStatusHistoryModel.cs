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
	/// Запись в истории изменения пользовательского статуса задачи
	/// </summary>
	public class CustomTaskStatusHistoryModel: SecureModel<Guid>, ITasksModel, IStatusHistoryRecordModel<TaskModel, CustomTaskStatusModel>
	{
		#region Persistent

		/// <summary>
		/// Дата изменения статуса на заданный
		/// </summary>
		[JsonProperty, NotNull/*, InRange(new DateTime(2000, 01, 01), new DateTime(2200, 01, 01))*/]
		public virtual DateTime Start { get; set; }

		/// <summary>
		/// Дата изменения статуса на следующий, фиксирует период нахождения
		/// задачи в заданном статусе. Может быть null, если задача все еще находится
		/// в заданном статусе
		/// </summary>
		[JsonProperty]
		public virtual DateTime? Finish { get; set; }

		/// <summary>
		/// Задачи, изменение статуса которой регистрируется
		/// </summary>
		[JsonProperty, NotNull]
		public virtual TaskModel Task { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? TaskId { get; set; }

		/// <summary>
		/// Установленный задаче статус
		/// </summary>
		[JsonProperty, NotNull]
		public virtual CustomTaskStatusModel Status { get; set; }

		#endregion
		
		[NotMapped]
		TaskModel IStatusHistoryRecordModel<TaskModel, CustomTaskStatusModel>.Holder
		{
			get { return Task; }
			set { Task = value; }
		}

		/// <summary>
		/// Открытая запись - запись о текущем статусе объекта
		/// </summary>
		[NotMapped]
		public virtual bool IsOpen
		{
			get { return !Finish.HasValue; }
		}
	}
}
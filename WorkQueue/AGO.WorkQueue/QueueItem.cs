using System;

namespace AGO.WorkQueue
{
	/// <summary>
	/// Запись в очереди задач
	/// </summary>
    public class QueueItem
    {
		public QueueItem(string type, Guid taskId, string project, string user)
			:this(type, taskId, project, user, DateTime.UtcNow)
		{
		}

		public QueueItem(string type, Guid taskId, string project, string user, DateTime cdate)
		{
			if (string.IsNullOrWhiteSpace(type))
				throw new ArgumentNullException("type");
			if (default(Guid).Equals(taskId))
				throw new ArgumentNullException("taskId");
			if (string.IsNullOrWhiteSpace(project))
				throw new ArgumentNullException("project");
			if (string.IsNullOrWhiteSpace(user))
				throw new ArgumentNullException("user");
			if (DateTime.MinValue.Equals(cdate))
				throw new ArgumentNullException("cdate");

			TaskType = type;
			TaskId = taskId;
			Project = project;
			User = user;
			CreateDate = cdate;
		}


		/// <summary>
		/// Тип задачи (пока только задачи для отчетов, 'Report')
		/// </summary>
		public string TaskType { get; private set; }

		/// <summary>
		/// Идентификатор задачи
		/// </summary>
		public Guid TaskId { get; private set; }

		/// <summary>
		/// Код проекта
		/// </summary>
		public string Project { get; private set; }

		/// <summary>
		/// Идентификатор пользователя, поставившего задачу
		/// </summary>
		public string User { get; private set; }

		/// <summary>
		/// Дата постановки задачи (UTC)
		/// </summary>
		public DateTime CreateDate { get; set; }

		/// <summary>
		/// Тип приоритета
		/// </summary>
		/// <remarks>0 - задачи без явно выделенного приоритета по пользователям. >0 - приоритет сначала по типу, потом по приоритету пользователя</remarks>
		public int PriorityType { get; set; }

		/// <summary>
		/// Приоритет пользователя в рамках проекта
		/// </summary>
		public int UserPriority { get; set; }

		/// <summary>
		/// № в общей очереди задач проекта
		/// </summary>
		public int? OrderInQueue { get; set; }

		public QueueItem Copy()
		{
			return new QueueItem(TaskType, TaskId, Project, User, CreateDate)
			{
				PriorityType = PriorityType,
				UserPriority = UserPriority
			};
		}
    }
}

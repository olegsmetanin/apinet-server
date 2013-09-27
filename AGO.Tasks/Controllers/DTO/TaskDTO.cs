using System;
using System.Collections.Generic;

namespace AGO.Tasks.Controllers.DTO
{
	public class BaseTaskDTO : ModelDTO
	{
		public string SeqNumber { get; set; }

		public string TaskType { get; set; }

		public string Content { get; set; }

		public IEnumerable<Executor> Executors { get; set; }

		public DateTime? DueDate { get; set; }

		public string Status { get; set; }

		public string CustomStatus { get; set; }

		public class Executor
		{
			public string Name { get; set; }

			public string Description { get; set; }
		}
	}

	public class TaskListItemDTO: BaseTaskDTO
	{
	}

	public class TaskViewDTO: BaseTaskDTO
	{
		public string Author { get; set; }

		public DateTime? CreationTime { get; set; }
	}
}
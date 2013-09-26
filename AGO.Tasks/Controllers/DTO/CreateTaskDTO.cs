using System;
using AGO.Core.Attributes.Constraints;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Controllers.DTO
{
	public class CreateTaskDTO
	{
		[NotEmpty]
		public Guid TaskType { get; set; }

		[NotEmpty]
		public Guid[] Executors { get; set; }

		public DateTime? DueDate { get; set; }

		public string Content { get; set; }

		public Guid? CustomStatus { get; set; }

		public TaskPriority Priority { get; set; }
	}
}
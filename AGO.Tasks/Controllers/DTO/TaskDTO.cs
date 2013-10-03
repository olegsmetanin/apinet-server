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
	}

	public class TaskListItemDTO: BaseTaskDTO
	{
	}

	public class TaskListItemDetailsDTO
	{
		public string Priority { get; set; }

		public string Content { get; set; }

		public string Note { get; set; }

		public IEnumerable<AgreementView> Agreements { get; set; }

		public IEnumerable<string> Files { get; set; }
	}

	public class TaskViewDTO: BaseTaskDTO
	{
		public string Priority { get; set; }

		public IEnumerable<Agreement> Agreements { get; set; }

		public StatusHistoryDTO StatusHistory { get; set; }

		public StatusHistoryDTO CustomStatusHistory { get; set; }

		public IEnumerable<CustomParameterDTO> Parameters { get; set; }

		public string Author { get; set; }

		public DateTime? CreationTime { get; set; }
	}

	public class Executor
	{
		public string Name { get; set; }

		public string Description { get; set; }
	}

	public class AgreementView
	{
		public string Agreemer { get; set; }

		public bool Done { get; set; }
	}

	public class Agreement: AgreementView
	{
		public DateTime? AgreedAt { get; set; }

		public string Comment;
	}
}
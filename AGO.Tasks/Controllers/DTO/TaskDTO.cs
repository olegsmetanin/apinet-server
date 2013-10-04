using System;
using System.Collections.Generic;
using AGO.Core.Controllers;
using Newtonsoft.Json;

namespace AGO.Tasks.Controllers.DTO
{
	public class BaseTaskDTO : ModelDTO
	{
		public string SeqNumber { get; set; }

		public string TaskType { get; set; }

		public string Content { get; set; }

		public IEnumerable<Executor> Executors { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
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
		public new LookupEntry TaskType { get; set; }

		public new LookupEntry Status { get; set; }

		public LookupEntry Priority { get; set; }

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

	public class Agreement: ModelDTO
	{
		public string Agreemer { get; set; }

		public bool Done { get; set; }

		public DateTime? AgreedAt { get; set; }

		public string Comment;
	}

	public class TaskPropChangeDTO: ModelDTO
	{
		public TaskPropChangeDTO()
		{
		}

		public TaskPropChangeDTO(Guid id, int? version, string prop, object value = null)
		{
			Id = id;
			ModelVersion = version;
			Prop = prop;
			Value = value;
		}

		public string Prop { get; set; }

		public object Value { get; set; }
	}
}
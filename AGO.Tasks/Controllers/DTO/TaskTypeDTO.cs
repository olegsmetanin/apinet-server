using System;

namespace AGO.Tasks.Controllers.DTO
{
	public class TaskTypeDTO
	{
		public Guid Id { get; set; }

		public string Name { get; set; }

		public string Author { get; set; }

		public DateTime? CreationTime { get; set; }

		public int? ModelVersion { get; set; }
	}
}
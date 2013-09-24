using System;

namespace AGO.Tasks.Controllers.DTO
{
	public class CustomStatusDTO
	{
		public Guid Id { get; set; }

		public string Name { get; set; }

		public byte ViewOrder { get; set; }

		public string Author { get; set; }

		public DateTime? CreationTime { get; set; }

		public int? ModelVersion { get; set; }
	}
}
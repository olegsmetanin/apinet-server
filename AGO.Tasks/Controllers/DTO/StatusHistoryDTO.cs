using System;
using AGO.Core.Controllers;

namespace AGO.Tasks.Controllers.DTO
{
	public class StatusHistoryDTO
	{
		public StatusHistoryItemDTO[] History;

		public string Current { get; set; }

		public LookupEntry[] Next { get; set; }
		
		public class StatusHistoryItemDTO
		{
			public string Status { get; set; }

			public DateTime Start { get; set; }

			public DateTime? Finish { get; set; }

			public string Author { get; set; }
		}
	}
}
using System;
using AGO.Core.Controllers;

namespace AGO.Tasks.Controllers.DTO
{
	/// <summary>
	/// DTO for view and change project settings
	/// </summary>
	public class ProjectDTO: ModelDTO
	{
		public string Name { get; set; }

		public string Author { get; set; }

		public DateTime? CreationTime { get; set; }

		public string ProjectCode { get; set; }

		public string Type { get; set; }

		public LookupEntry Status { get; set; }

		public string Description { get; set; }

		public bool VisibleForAll { get; set; }

		public LookupEntry[] Tags { get; set; }
	}
}
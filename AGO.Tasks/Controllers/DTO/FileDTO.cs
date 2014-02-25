using System;

namespace AGO.Tasks.Controllers.DTO
{
	/// <summary>
	/// Task file DTO
	/// </summary>
	public class FileDTO: ModelDTO
	{
		public string Author { get; set; }

		public DateTime? CreationTime { get; set; }

		public string Name { get; set; }

		public long Size { get; set; }

		public bool Uploaded { get; set; }
	}
}
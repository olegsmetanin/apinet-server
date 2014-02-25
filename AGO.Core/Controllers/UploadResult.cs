using System.Collections.Generic;
using Newtonsoft.Json;

namespace AGO.Core.Controllers
{
	/// <summary>
	/// Result of file upload operation
	/// </summary>
	public class UploadResult<TModel>
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("length")]
		public long Length { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("model")]
		public TModel Model { get; set; }
	}

	public class UploadedFiles<TModel>
	{
		[JsonProperty("files")]
		public IEnumerable<UploadResult<TModel>> Files { get; set; }
	}
}
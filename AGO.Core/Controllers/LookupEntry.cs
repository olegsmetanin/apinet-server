using Newtonsoft.Json;

namespace AGO.Core.Controllers
{
	public struct LookupEntry
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }
	}
}

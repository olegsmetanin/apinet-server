using Newtonsoft.Json.Linq;

namespace AGO.Core.Json
{
	internal class JsonRequest : IJsonRequest
	{
		public string Project { get; set; }

		public JObject Body { get; set; }

		public bool DontFetchReferences { get; set; }
	}
}
using Newtonsoft.Json.Linq;

namespace AGO.Docstore.Json
{
	internal class JsonRequest : IJsonRequest
	{
		public JObject Body { get; set; }

		public bool DontFetchReferences { get; set; }
	}
}
using Newtonsoft.Json.Linq;

namespace AGO.Core.Json
{
	internal class JsonRequest : IJsonRequest
	{
		public JObject Body { get; set; }

		public bool DontFetchReferences { get; set; }
	}
}
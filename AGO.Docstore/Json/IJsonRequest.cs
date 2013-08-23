using Newtonsoft.Json.Linq;

namespace AGO.Docstore.Json
{
	public interface IJsonRequest
	{
		JObject Body { get; }

		bool DontFetchReferences { get; }
	}
}
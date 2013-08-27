using Newtonsoft.Json.Linq;

namespace AGO.Core.Json
{
	public interface IJsonRequest
	{
		JObject Body { get; }

		bool DontFetchReferences { get; }
	}
}
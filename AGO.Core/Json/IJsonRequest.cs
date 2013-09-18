using Newtonsoft.Json.Linq;

namespace AGO.Core.Json
{
	public interface IJsonRequest
	{
		string Project { get; }

		JObject Body { get; }

		bool DontFetchReferences { get; }
	}
}
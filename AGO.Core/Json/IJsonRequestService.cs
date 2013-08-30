using Newtonsoft.Json;

namespace AGO.Core.Json
{
	public interface IJsonRequestService
	{
		IJsonRequest ParseRequest(JsonReader reader);

		IJsonModelsRequest ParseModelsRequest(JsonReader reader, int defaultPageSize, int maxPageSize);

		IJsonModelRequest<TIdType> ParseModelRequest<TIdType>(JsonReader reader);
	}
}
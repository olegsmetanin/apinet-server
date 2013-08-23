using System.IO;
using Newtonsoft.Json;

namespace AGO.Docstore.Json
{
	public interface IJsonRequestService
	{
		IJsonModelsRequest ParseModelsRequest(JsonReader reader, int defaultPageSize, int maxPageSize);

		IJsonModelsRequest ParseModelsRequest(TextReader reader, int defaultPageSize, int maxPageSize);

		IJsonModelsRequest ParseModelsRequest(string str, int defaultPageSize, int maxPageSize);

		IJsonModelsRequest ParseModelsRequest(Stream stream, int defaultPageSize, int maxPageSize);

		IJsonModelRequest<TIdType> ParseModelRequest<TIdType>(JsonReader reader);

		IJsonModelRequest<TIdType> ParseModelRequest<TIdType>(TextReader reader);

		IJsonModelRequest<TIdType> ParseModelRequest<TIdType>(string str);

		IJsonModelRequest<TIdType> ParseModelRequest<TIdType>(Stream stream);
	}
}
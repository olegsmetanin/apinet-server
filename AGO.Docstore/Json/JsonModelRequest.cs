namespace AGO.Docstore.Json
{
	internal class JsonModelRequest<TIdType> : JsonRequest, IJsonModelRequest<TIdType>
	{
		public TIdType Id { get; set; }
	}
}

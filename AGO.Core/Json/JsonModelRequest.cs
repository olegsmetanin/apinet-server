namespace AGO.Core.Json
{
	internal class JsonModelRequest<TIdType> : JsonRequest, IJsonModelRequest<TIdType>
	{
		public TIdType Id { get; set; }
	}
}

namespace AGO.Docstore.Json
{
	public interface IJsonModelRequest<out TIdType> : IJsonRequest
	{
		TIdType Id { get; }
	}
}

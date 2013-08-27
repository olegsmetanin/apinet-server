namespace AGO.Core.Json
{
	public interface IJsonModelRequest<out TIdType> : IJsonRequest
	{
		TIdType Id { get; }
	}
}

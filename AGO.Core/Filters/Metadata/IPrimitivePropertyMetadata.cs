namespace AGO.Core.Filters.Metadata
{
	public interface IPrimitivePropertyMetadata : IPropertyMetadata
	{
		bool IsTimestamp { get; set; }
	}
}

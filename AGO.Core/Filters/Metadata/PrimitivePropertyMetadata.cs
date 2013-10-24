namespace AGO.Core.Filters.Metadata
{
	internal class PrimitivePropertyMetadata : PropertyMetadata, IPrimitivePropertyMetadata
	{
		public bool IsTimestamp { get; set; }
	}
}

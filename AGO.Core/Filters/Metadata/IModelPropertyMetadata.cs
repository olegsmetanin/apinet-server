namespace AGO.Core.Filters.Metadata
{
	public interface IModelPropertyMetadata : IPropertyMetadata
	{
		bool IsCollection { get; }
	}
}

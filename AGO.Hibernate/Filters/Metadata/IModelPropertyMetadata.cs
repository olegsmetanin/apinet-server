namespace AGO.Hibernate.Filters.Metadata
{
	public interface IModelPropertyMetadata : IPropertyMetadata
	{
		bool IsCollection { get; }
	}
}

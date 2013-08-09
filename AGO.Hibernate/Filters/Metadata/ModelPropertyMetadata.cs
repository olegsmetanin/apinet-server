namespace AGO.Hibernate.Filters.Metadata
{
	internal class ModelPropertyMetadata : PropertyMetadata, IModelPropertyMetadata
	{
		public bool IsCollection { get; set; }
	}
}

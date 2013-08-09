using System;

namespace AGO.Hibernate.Filters.Metadata
{
	internal class PropertyMetadata : IPropertyMetadata
	{
		public string Name { get; set; }

		public string DisplayName { get; set; }

		public Type PropertyType { get; set; }
	}
}

using System;

namespace AGO.Hibernate.Filters.Metadata
{
	public interface IPropertyMetadata
	{
		string Name { get; }

		string DisplayName { get; }

		Type PropertyType { get; }
	}
}

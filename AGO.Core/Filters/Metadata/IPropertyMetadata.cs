using System;

namespace AGO.Core.Filters.Metadata
{
	public interface IPropertyMetadata
	{
		string Name { get; }

		string DisplayName { get; }

		Type PropertyType { get; }
	}
}

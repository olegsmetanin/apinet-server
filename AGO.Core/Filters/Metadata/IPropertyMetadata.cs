using System;

namespace AGO.Core.Filters.Metadata
{
	public interface IPropertyMetadata
	{
		string Name { get; }

		Type PropertyType { get; }
	}
}

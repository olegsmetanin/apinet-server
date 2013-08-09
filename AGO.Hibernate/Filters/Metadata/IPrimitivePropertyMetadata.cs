using System.Collections.Generic;

namespace AGO.Hibernate.Filters.Metadata
{
	public interface IPrimitivePropertyMetadata : IPropertyMetadata
	{
		bool IsTimestamp { get; set; }

		IDictionary<string, string> PossibleValues { get; }
	}
}

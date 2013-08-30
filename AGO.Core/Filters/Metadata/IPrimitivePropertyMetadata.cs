using System.Collections.Generic;

namespace AGO.Core.Filters.Metadata
{
	public interface IPrimitivePropertyMetadata : IPropertyMetadata
	{
		bool IsTimestamp { get; set; }

		IDictionary<string, string> PossibleValues { get; }
	}
}

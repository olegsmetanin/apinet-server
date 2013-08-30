using System.Collections.Generic;

namespace AGO.Core.Filters.Metadata
{
	internal class PrimitivePropertyMetadata : PropertyMetadata, IPrimitivePropertyMetadata
	{
		public bool IsTimestamp { get; set; }

		public IDictionary<string, string> PossibleValues { get; set; }
	}
}

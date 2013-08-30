using System;
using System.Collections.Generic;

namespace AGO.Core.Filters.Metadata
{
	public interface IModelMetadata
	{
		string Name { get; }

		Type ModelType { get; }

		IEnumerable<IPrimitivePropertyMetadata> PrimitiveProperties { get; }

		IEnumerable<IModelPropertyMetadata> ModelProperties { get; }
	}
}

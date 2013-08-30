using System;
using System.Collections.Generic;

namespace AGO.Core.Filters.Metadata
{
	internal class ModelMetadata : IModelMetadata
	{
		public string Name { get; set; }

		public Type ModelType { get; set; }

		readonly internal IList<IPrimitivePropertyMetadata> _PrimitiveProperties = new List<IPrimitivePropertyMetadata>();
		public IEnumerable<IPrimitivePropertyMetadata> PrimitiveProperties { get { return _PrimitiveProperties; } }

		readonly internal IList<IModelPropertyMetadata> _ModelProperties = new List<IModelPropertyMetadata>();
		public IEnumerable<IModelPropertyMetadata> ModelProperties { get { return _ModelProperties; } }
	}
}

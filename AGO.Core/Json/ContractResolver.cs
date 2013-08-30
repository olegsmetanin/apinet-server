using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using AGO.Core.Filters.Metadata;
using AGO.Core.Model;

namespace AGO.Core.Json
{
	public class ContractResolver : DefaultContractResolver
	{
		protected override JsonConverter ResolveContractConverter(Type objectType)
		{
			if (typeof (IIdentifiedModel).IsAssignableFrom(objectType))
				return new IdentifiedModelConverter();
			
			if (typeof(IEnumerable<IModelMetadata>).IsAssignableFrom(objectType))
				return new ModelMetadataConverter();

			return base.ResolveContractConverter(objectType);
		}
	}
}
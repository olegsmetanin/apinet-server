using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using AGO.Hibernate.Filters.Metadata;
using AGO.Hibernate.Model;

namespace AGO.Hibernate.Json
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
using System;

namespace AGO.Core.Attributes.Mapping
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
	public class MetadataExcludeAttribute : Attribute
	{
	}
}

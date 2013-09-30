using System;

namespace AGO.Core.Attributes.Constraints
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class UniquePropertyAttribute: Attribute
	{
		public string Group { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace AGO.Core.Attributes.Constraints
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class UniquePropertyAttribute: Attribute
	{
		protected readonly IList<string> _GroupProperties;
		public IList<string> GroupProperties { get { return _GroupProperties; } }

		public UniquePropertyAttribute(params string[] groupProperties)
		{
			_GroupProperties = new List<string>(
				(groupProperties ?? Enumerable.Empty<string>()).Where(s => !s.IsNullOrWhiteSpace()));
		}
	}
}

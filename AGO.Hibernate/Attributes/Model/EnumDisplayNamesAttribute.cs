using System;
using System.Collections.Generic;

namespace AGO.Hibernate.Attributes.Model
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class EnumDisplayNamesAttribute : Attribute
	{
		private readonly IDictionary<string, string> _DisplayNames = new Dictionary<string, string>();
		public IDictionary<string, string> DisplayNames { get { return _DisplayNames; } }
 
		public EnumDisplayNamesAttribute(string[] displayNames)
		{
			if (displayNames == null)
				throw new ArgumentNullException("displayNames");

			string key = null;
			foreach (var str in displayNames)
			{
				if (key == null)
				{
					key = str.TrimSafe();
					continue;
				}

				_DisplayNames[key] = str.TrimSafe();
				key = null;
			}
		}
	}
}

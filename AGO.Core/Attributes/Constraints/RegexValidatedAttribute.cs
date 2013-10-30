using System;
using System.Text.RegularExpressions;

namespace AGO.Core.Attributes.Constraints
{
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = true)]
	public class RegexValidatedAttribute: Attribute
	{
		public Regex Regex { get; private set; }

		public RegexValidatedAttribute(string regex)
		{
			if (regex.IsNullOrWhiteSpace())
				throw new ArgumentNullException();

			Regex = new Regex(regex);
		}
	}
}

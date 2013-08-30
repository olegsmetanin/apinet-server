using System;
using System.Collections.Generic;

namespace AGO.Core.Attributes.Controllers
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
	public class RequireAuthorizationAttribute : Attribute
	{
		public ISet<string> RequiredRoles { get; private set; }

		public RequireAuthorizationAttribute(params string[] requiredRoles)
		{
			RequiredRoles = new HashSet<string>(requiredRoles ?? new string[0]);
		}
	}
}

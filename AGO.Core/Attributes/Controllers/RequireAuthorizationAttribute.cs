using System;

namespace AGO.Core.Attributes.Controllers
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
	public class RequireAuthorizationAttribute : Attribute
	{
		public bool RequireAdmin { get; private set; }

		public RequireAuthorizationAttribute(bool requireAdmin = false)
		{
			RequireAdmin = requireAdmin;
		}
	}
}

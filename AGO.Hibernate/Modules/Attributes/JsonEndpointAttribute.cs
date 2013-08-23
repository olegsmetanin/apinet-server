using System;

namespace AGO.Hibernate.Modules.Attributes
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class JsonEndpointAttribute : Attribute
	{
	}
}

using System;

namespace AGO.Core.Modules.Attributes
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class JsonEndpointAttribute : Attribute
	{
	}
}

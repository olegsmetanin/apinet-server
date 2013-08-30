using System;

namespace AGO.Core
{
	public class ServiceNotInitializedException : Exception
	{
	}

	public class NotAuthorizedException : Exception
	{
		public NotAuthorizedException()
			: base("Authorization required")
		{
		}
	}

	public class AccessForbiddenException : Exception
	{
		public AccessForbiddenException()
			: base("Not enough access rights")
		{
		}
	}
}
using System;

namespace AGO.Core
{
	public class CannotDeleteReferencedItemException : Exception
	{
		public CannotDeleteReferencedItemException()
			: base("Cannot delete item, while other items reference it")
		{
		}
	}

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

	public class RequiredFieldException : Exception
	{
		public RequiredFieldException()
			: base("Required field")
		{
		}
	}

	public class UniqueFieldException : Exception
	{
		public UniqueFieldException()
			: base("Must be unique")
		{
		}
	}

	public class MalformedRequestException : Exception
	{
		public MalformedRequestException()
			: base("Malformed request")
		{
		}
	}
}
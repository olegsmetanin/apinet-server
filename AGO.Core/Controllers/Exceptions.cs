using System;

namespace AGO.Core.Controllers
{
	public class EmptyLoginException : Exception
	{
		public EmptyLoginException()
			: base("Empty login")
		{
		}
	}

	public class EmptyPwdException : Exception
	{
		public EmptyPwdException()
			: base("Empty pwd")
		{
		}
	}

	public class NoSuchUserException : Exception
	{
		public NoSuchUserException()
			: base("No such user in database")
		{
		}
	}

	public class InvalidPwdException : Exception
	{
		public InvalidPwdException()
			: base("Invalid password")
		{
		}
	}
}

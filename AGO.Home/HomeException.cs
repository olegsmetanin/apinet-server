using System;
using AGO.Core;

namespace AGO.Home
{
	public abstract class HomeException : AbstractApplicationException
	{
		protected HomeException(string message = null, Exception innerException = null)
			: base(message, innerException)
		{
		}
	}

	public class NoInitialProjectStatusException : HomeException
	{
	}

	public class NoSuchProjectException: HomeException
	{
	}
}
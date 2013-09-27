using System;
using System.Collections.Generic;
using AGO.Core.Localization;

namespace AGO.Core
{
	public abstract class AbstractApplicationException : Exception, ILocalizable
	{
		protected IList<object> _MessageArguments = new List<object>();
		public virtual IEnumerable<object> MessageArguments { get { return _MessageArguments; } }

		protected AbstractApplicationException(string message = null, Exception innerException = null)
			: base(message, innerException)
		{
		}
	}

	public class NotAuthenticatedException : AbstractApplicationException
	{
	}

	public class AccessForbiddenException : AbstractApplicationException
	{
	}

	public class CannotDeleteReferencedItemException : AbstractApplicationException
	{
	}

	public class RequiredValueException : AbstractApplicationException
	{
	}

	public class MustBeUniqueException : AbstractApplicationException
	{
	}

	public class MustBeGreaterThanException : AbstractApplicationException
	{
		public MustBeGreaterThanException(object value)
		{
			_MessageArguments.Add(value);
		}
	}

	public class MustBeGreaterOrEqualToException : AbstractApplicationException
	{
		public MustBeGreaterOrEqualToException(object value)
		{
			_MessageArguments.Add(value);
		}
	}

	public class MustBeLowerThanException : AbstractApplicationException
	{
		public MustBeLowerThanException(object value)
		{
			_MessageArguments.Add(value);
		}
	}

	public class MustBeLowerOrEqualToException : AbstractApplicationException
	{
		public MustBeLowerOrEqualToException(object value)
		{
			_MessageArguments.Add(value);
		}
	}

	public class MustBeBetweenException : AbstractApplicationException
	{
		public MustBeBetweenException(object start, object end)
		{
			_MessageArguments.Add(start);
			_MessageArguments.Add(end);
		}
	}

	public class MustBeInRangeException : AbstractApplicationException
	{
		public MustBeInRangeException(object start, object end)
		{
			_MessageArguments.Add(start);
			_MessageArguments.Add(end);
		}
	}

	public class DataAccessException : AbstractApplicationException
	{
		public DataAccessException(Exception innerException)
			: base(null, innerException)
		{
		}
	}

	public class NoSuchEntityException : AbstractApplicationException
	{
	}
}
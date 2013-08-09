using System;

namespace AGO.Hibernate.Filters
{
	public class FilteringException : Exception
	{
		public FilteringException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	public class FilterValidationException : FilteringException
	{
		public FilterValidationException(Exception innerException)
			: base("Filter validation failure", innerException)
		{
		}
	}

	public class FilterJsonException : FilteringException
	{
		public FilterJsonException(Exception innerException)
			: base("Filter to json or json to filter failure", innerException)
		{
		}
	}

	public class FilterConcatenationException : FilteringException
	{
		public FilterConcatenationException(Exception innerException)
			: base("Filter concatenation exception", innerException)
		{
		}
	}

	public class FilterCompilationException : FilteringException
	{
		public FilterCompilationException(Exception innerException)
			: base("Filter compilation exception", innerException)
		{
		}
	}
}
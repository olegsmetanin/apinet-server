using System;
using System.Reflection;

namespace AGO.Core.Filters
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
			: base("Filter serialization/deserialization failure", innerException)
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

	public class EmptyNodePathException : Exception
	{
		public override string Message
	{
			get { return "Filter node path not specified"; }
	}
	}

	public class UnexpectedTypeException : Exception
	{
		public UnexpectedTypeException(Type type)
			: base(string.Format("Unexpected model or property type \"{0}\"", type))
		{
		}
	}

	public class InvalidValueFilterOperatorException : Exception
	{
		public InvalidValueFilterOperatorException(ValueFilterOperators op, PropertyInfo propertyInfo)
			: base(string.Format("Invalid filter operator \"{0}\" for property \"{1}\"", op, propertyInfo))
		{
		}
	}

	public class InvalidFilterOperandException : Exception
	{
		public InvalidFilterOperandException(PropertyInfo propertyInfo)
			: base(string.Format("Invalid operand value for property \"{0}\"", propertyInfo))
		{
		}
	}

	public class MissingModelPropertyException : Exception
	{
		public MissingModelPropertyException(string name, Type modelType)
			: base(string.Format("Property \"{0}\" not found in model \"{1}\"", name, modelType))
		{
		}
	}

	public class NotMappedModelPropertyException : Exception
	{
		public NotMappedModelPropertyException(PropertyInfo propertyInfo)
			: base(string.Format("Model property \"{0}\" is not mapped to database", propertyInfo))
		{
		}
	}

	public class EndJoinWithoutJoinException : Exception
	{
		public override string Message
		{
			get { return "EndJoin without join"; }
		}
	}

	public class EmptyFilterDeserializationResultException : Exception
	{
		public override string Message
	{
			get { return "Filter deserialization procedure returned empty result"; }
		}
	}

	public class DeepEndJoinException : Exception
	{
	}
}

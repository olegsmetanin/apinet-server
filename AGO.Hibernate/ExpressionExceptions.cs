using System;

namespace AGO.Hibernate
{
	public class UnexpectedExpressionTypeException : Exception
	{
		public Type ActualType { get; private set; }

		public UnexpectedExpressionTypeException(Type actualType)
			: base(string.Format("Unexpected expression type: {0}", actualType))
		{
			ActualType = actualType;
		}
	}

	public class UnexpectedExpressionTypeException<TExpected> : Exception
	{
		public Type ActualType { get; private set; }

		public UnexpectedExpressionTypeException(Type actualType)
			: base(string.Format("Unexpected expression type, expected: {0}, actual: {1}", typeof(TExpected), actualType))
		{
			ActualType = actualType;
		}
	}

	public class FilterAlreadyContainsNodeWithSameNameException : Exception
	{
		public string PropertyName { get; private set; }

		public FilterAlreadyContainsNodeWithSameNameException(string propertyName)
			: base(string.Format("Unable to add model node, filter already contains node with name {0}", propertyName))
		{
			PropertyName = propertyName;
		}
	}

	public class UnexpectedUnaryExpressionNodeTypeException : Exception
	{
	}

	public class UnexpectedBinaryExpressionNodeTypeException : Exception
	{
	}

	public class UnexpectedMemberExpressionPropertyTypeException : Exception
	{
	}

	public class PropertyAccessExpressionExpectedException : Exception
	{
	}

	public class OnlyModelPropertiesCanBeInChainException : Exception
	{
	}
}

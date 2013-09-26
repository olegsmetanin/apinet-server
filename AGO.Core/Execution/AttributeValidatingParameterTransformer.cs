using System;
using System.Reflection;
using AGO.Core.Attributes.Constraints;

namespace AGO.Core.Execution
{
	public class AttributeValidatingParameterTransformer : IActionParameterTransformer
	{
		#region Interfaces implementation

		public bool Accepts(
			ParameterInfo parameterInfo,
			object parameterValue)
		{
			return true;
		}

		public object Transform(
			ParameterInfo parameterInfo, 
			object parameterValue)
		{
			try
			{
			var result = parameterValue;
			var paramType = parameterInfo.ParameterType;

			if (result != null)
			{
				if (!paramType.IsInstanceOfType(result))
					result = result.ConvertSafe(paramType);
			}
			else if (paramType.IsValueType)
				result = Activator.CreateInstance(paramType);

			var invalidAttribute = parameterInfo.FindInvalidParameterConstraintAttribute(result);
				if (invalidAttribute == null)
					return result;

				if (invalidAttribute is NotNullAttribute || invalidAttribute is NotEmptyAttribute)
					throw new RequiredValueException();

				var inRange = invalidAttribute as InRangeAttribute;
				if (inRange != null && inRange.Inclusive)
				{
					if (inRange.Start != null && inRange.End != null)
						throw new MustBeInRangeException(inRange.Start, inRange.End);
					if (inRange.Start != null)
						throw new MustBeGreaterOrEqualToException(inRange.Start);
					if (inRange.End != null)
						throw new MustBeLowerOrEqualToException(inRange.End);
				}
				if (inRange != null && !inRange.Inclusive)
				{
					if (inRange.Start != null && inRange.End != null)
						throw new MustBeBetweenException(inRange.Start, inRange.End);
					if (inRange.Start != null)
						throw new MustBeGreaterThanException(inRange.Start);
					if (inRange.End != null)
						throw new MustBeLowerThanException(inRange.End);
			}

			return result;
		}
			catch (AbstractApplicationException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new ControllerActionParameterException(parameterInfo.Name, e);
			}
		}

		#endregion
	}
}
using System;
using System.Reflection;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Json;

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
			if (invalidAttribute != null)
			{
				if (invalidAttribute is NotNullAttribute || invalidAttribute is NotEmptyAttribute)
				{
					if (invalidAttribute is NotNullAttribute)
						throw new ArgumentNullException(parameterInfo.Name);

					throw new ArgumentException("Parameter is empty", parameterInfo.Name);
				}

				var inRange = invalidAttribute as InRangeAttribute;
				if (inRange != null)
					throw new ArgumentException(string.Format("Parameter not in range ({0} - {1}){2}",
				inRange.Start, inRange.End, inRange.Inclusive ? " inclusive" : string.Empty), parameterInfo.Name);

				throw new InvalidOperationException();
			}

			return result;
		}

		#endregion
	}
}
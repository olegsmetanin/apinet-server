using System;
using System.Reflection;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Json;
using Newtonsoft.Json.Linq;

namespace AGO.Core.Execution
{
	public class AttributeValidatingParameterTransformer : IActionParameterTransformer
	{
		#region Properties, fields, constructors
		
		protected readonly IJsonService _JsonService;

		public AttributeValidatingParameterTransformer(IJsonService jsonService)
		{
			if (jsonService == null)
				throw new ArgumentNullException("jsonService");
			_JsonService = jsonService;
		}

		#endregion

		#region Interfaces implementation

		public bool Accepts(
			ParameterInfo parameterInfo,
			object parameterValue)
		{
			return true;
		}

		public bool Transform(
			ParameterInfo parameterInfo, 
			ref object parameterValue)
		{
			var initial = parameterValue;
			var paramType = parameterInfo.ParameterType;

			var token = parameterValue as JToken;
			if (token != null)
			{
				var tokenReader = new JTokenReader(token) { CloseInput = false };
				parameterValue = _JsonService.CreateSerializer().Deserialize(tokenReader, paramType);
			}

			if (parameterValue != null)
			{
				if (!paramType.IsInstanceOfType(parameterValue))
					parameterValue = parameterValue.ConvertSafe(paramType);
			}
			else
			{
				if (paramType.IsValueType)
					parameterValue = Activator.CreateInstance(paramType);
			}

			var invalidAttribute = parameterInfo.FindInvalidParameterConstraintAttribute(parameterValue);
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
			return (parameterValue != null && paramType.IsInstanceOfType(parameterValue)) || initial == null;
		}

		#endregion
	}
}
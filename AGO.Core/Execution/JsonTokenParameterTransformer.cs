using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AGO.Core.Json;

namespace AGO.Core.Execution
{
	public class JsonTokenParameterTransformer : IActionParameterTransformer
	{
		#region Properties, fields, constructors
		
		protected readonly IJsonService _JsonService;

		public JsonTokenParameterTransformer(IJsonService jsonService)
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
			return parameterValue is JToken;
		}

		public object Transform(
			ParameterInfo parameterInfo,
			object parameterValue)
		{
			var value = parameterValue as JValue;
			if (value != null)
				return value.Value;

			try
			{
				var tokenReader = new JTokenReader((JToken) parameterValue) { CloseInput = false };
				return _JsonService.CreateSerializer().Deserialize(tokenReader, parameterInfo.ParameterType);
			}
			catch (JsonReaderException)
			{
			}

			return parameterValue;
		}

		#endregion
	}
}
using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.WebPages.Scope;
using AGO.Core.Execution;
using AGO.Core.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AGO.Core;

namespace AGO.WebApiApp.Controllers
{
	public class JsonBodyParameterResolver : IActionParameterResolver
	{
		#region Constants

		public const string JsonRequestBodyKey = "JsonRequestBody";

		#endregion

		#region Properties, fields, constructors
		
		protected readonly IJsonService _JsonService;

		public JsonBodyParameterResolver(IJsonService jsonService)
		{
			if (jsonService == null)
				throw new ArgumentNullException("jsonService");
			_JsonService = jsonService;
		}

		#endregion

		#region Interfaces implementation

		public bool Accepts(ParameterInfo parameterInfo)
		{
			if (HttpContext.Current.Request.ContentLength == 0)
				return false;

			var token = ScopeStorage.CurrentScope.ContainsKey(JsonRequestBodyKey)
				? ScopeStorage.CurrentScope[JsonRequestBodyKey] as JObject
				: null;
			if (token == null)
			{
				using (var jsonReader = _JsonService.CreateReader(
					new StreamReader(HttpContext.Current.Request.InputStream)))
				{
					try
					{
						token = JToken.ReadFrom(jsonReader) as JObject;
						ScopeStorage.CurrentScope[JsonRequestBodyKey] = token;
					}
					catch (JsonReaderException)
					{
					}
				}
			}

			return token != null && token.HasValues;
		}

		public bool Resolve(
			ParameterInfo parameterInfo, 
			out object parameterValue)
		{
			if (typeof (HttpFileCollection).IsAssignableFrom(parameterInfo.ParameterType))
			{
				parameterValue = HttpContext.Current.Request.Files;
				return true;
			}

			var jsonBody = ScopeStorage.CurrentScope.ContainsKey(JsonRequestBodyKey)
				? ScopeStorage.CurrentScope[JsonRequestBodyKey] as JObject
				: null;

			if (typeof(JsonReader).IsAssignableFrom(parameterInfo.ParameterType))
				parameterValue = new JTokenReader(jsonBody) { CloseInput = false };
			else
			{
				var jsonProperty = jsonBody != null ? jsonBody.Property(parameterInfo.Name) : null;

				parameterValue = jsonProperty != null
					? (jsonProperty.Value is JValue ? (object)jsonProperty.TokenValue() : jsonProperty.Value)
					: null;
			}

			return parameterValue != null;
		}

		#endregion
	}
}
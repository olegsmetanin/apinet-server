using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Linq;
using AGO.Core;
using AGO.Core.Execution;
using AGO.Core.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AGO.WebApiApp.Execution
{
	public class FormOrQueryParameterResolver : IActionParameterResolver
	{
		#region Properties, fields, constructors
		
		protected readonly IJsonService _JsonService;

		public FormOrQueryParameterResolver(IJsonService jsonService)
		{
			if (jsonService == null)
				throw new ArgumentNullException("jsonService");
			_JsonService = jsonService;
		}

		#endregion

		#region Interfaces implementation

		public bool Accepts(ParameterInfo parameterInfo)
		{
			return HttpContext.Current.Request.Form.AllKeys.Any(parameterInfo.Name.Equals) ||
				HttpContext.Current.Request.QueryString.AllKeys.Any(parameterInfo.Name.Equals);
		}

		public bool Resolve(
			ParameterInfo parameterInfo, 
			out object parameterValue)
		{
			parameterValue = HttpContext.Current.Request.Form[parameterInfo.Name] ??
				HttpContext.Current.Request.QueryString[parameterInfo.Name];

			if (parameterValue is string && !typeof (string).IsAssignableFrom(parameterInfo.ParameterType) &&
			    !parameterInfo.ParameterType.IsValue())
			{
				using (var jsonReader = _JsonService.CreateReader(new StringReader((string) parameterValue)))
				{
					try
					{
						parameterValue = JToken.ReadFrom(jsonReader);
					}
					catch (JsonReaderException)
					{
					}
				}	
			}

			return parameterValue != null;
		}

		#endregion
	}
}
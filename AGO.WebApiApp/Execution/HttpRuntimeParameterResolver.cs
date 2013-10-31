using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Web;
using AGO.Core.Execution;

namespace AGO.WebApiApp.Execution
{
	public class HttpRuntimeParameterResolver : IActionParameterResolver
	{
		#region Constants

		public const string RequestHeaders = "RequestHeaders";

		public const string ResponseHeaders = "ResponseHeaders";

		public const string UserLanguages = "UserLanguages";

		#endregion

		#region Interfaces implementation

		public bool Accepts(ParameterInfo parameterInfo)
		{
			var type = parameterInfo.ParameterType;

			return
				typeof (HttpContextBase).IsAssignableFrom(type) ||
				typeof (HttpRequestBase).IsAssignableFrom(type) ||
				typeof (HttpResponseBase).IsAssignableFrom(type) ||
				typeof (HttpFileCollectionBase).IsAssignableFrom(type) ||
				typeof (NameValueCollection).IsAssignableFrom(type) && (
					RequestHeaders.Equals(parameterInfo.Name, StringComparison.InvariantCultureIgnoreCase) ||
					ResponseHeaders.Equals(parameterInfo.Name, StringComparison.InvariantCultureIgnoreCase)) ||
				typeof (IEnumerable<string>).IsAssignableFrom(type) &&
					UserLanguages.Equals(parameterInfo.Name, StringComparison.InvariantCultureIgnoreCase);
		}

		public bool Resolve(
			ParameterInfo parameterInfo, 
			out object parameterValue)
		{
			var type = parameterInfo.ParameterType;
			parameterValue = null;

			if (typeof (HttpContextBase).IsAssignableFrom(type))
				parameterValue = new HttpContextWrapper(HttpContext.Current);
			if (typeof(HttpRequestBase).IsAssignableFrom(type))
				parameterValue = new HttpRequestWrapper(HttpContext.Current.Request);
			if (typeof(HttpResponseBase).IsAssignableFrom(type))
				parameterValue = new HttpResponseWrapper(HttpContext.Current.Response);
			if (typeof(HttpFileCollectionBase).IsAssignableFrom(type))
				parameterValue = new HttpFileCollectionWrapper(HttpContext.Current.Request.Files);
			if (typeof (NameValueCollection).IsAssignableFrom(type))
			{
				if (RequestHeaders.Equals(parameterInfo.Name, StringComparison.InvariantCultureIgnoreCase))
					parameterValue = HttpContext.Current.Request.Headers;
				if (ResponseHeaders.Equals(parameterInfo.Name, StringComparison.InvariantCultureIgnoreCase))
					parameterValue = HttpContext.Current.Response.Headers;
			}
			if (typeof (IEnumerable<string>).IsAssignableFrom(type) &&
					UserLanguages.Equals(parameterInfo.Name, StringComparison.InvariantCultureIgnoreCase))
				parameterValue = HttpContext.Current.Request.UserLanguages;

			return parameterValue != null;
		}

		#endregion
	}
}
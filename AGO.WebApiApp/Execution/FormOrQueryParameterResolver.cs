using System.Reflection;
using System.Web;
using System.Linq;
using AGO.Core.Execution;

namespace AGO.WebApiApp.Execution
{
	public class FormOrQueryParameterResolver : IActionParameterResolver
	{
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

			return parameterValue != null;
		}

		#endregion
	}
}
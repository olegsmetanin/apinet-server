using System;
using System.Collections.Generic;
using System.Reflection;
using AGO.Core.Filters;
using Newtonsoft.Json.Linq;

namespace AGO.Core.Execution
{
	public class FilterParameterTransformer : IActionParameterTransformer
	{
		#region Properties, fields, constructors
		
		protected readonly IFilteringService _FilteringService;

		public FilterParameterTransformer(IFilteringService filteringService)
		{
			if (filteringService == null)
				throw new ArgumentNullException("filteringService");
			_FilteringService = filteringService;
		}

		#endregion

		#region Interfaces implementation

		public bool Accepts(
			ParameterInfo parameterInfo,
			object parameterValue)
		{
			return parameterInfo.ParameterType.IsAssignableFrom(typeof(List<IModelFilterNode>)) &&
				parameterValue is JObject;
		}

		public object Transform(
			ParameterInfo parameterInfo, 
			object parameterValue)
		{
			var filterObject = (JObject) parameterValue;

			return _FilteringService.ParseFilterSetFromJson(filterObject.ToString());
		}

		#endregion
	}
}
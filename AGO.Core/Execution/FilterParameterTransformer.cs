using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AGO.Core.Filters;
using AGO.Core.Json;
using Newtonsoft.Json.Linq;

namespace AGO.Core.Execution
{
	public class FilterParameterTransformer : IActionParameterTransformer
	{
		#region Constants

		public const string SimpleFilterName = "simple";

		public const string ComplexFilterName = "complex";

		public const string UserFilterName = "user";

		#endregion

		#region Properties, fields, constructors
		
		protected readonly IJsonService _JsonService;

		protected readonly IFilteringService _FilteringService;

		public FilterParameterTransformer(
			IJsonService jsonService,
			IFilteringService filteringService)
		{
			if (jsonService == null)
				throw new ArgumentNullException("jsonService");
			_JsonService = jsonService;

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
			var result = new List<IModelFilterNode>();

			if (filterObject != null)
			{
				var simpleFilterProperty = filterObject.Property(SimpleFilterName);
				if (simpleFilterProperty != null)
					result.Add(ParseSimpleFilter(simpleFilterProperty));

				var complexFilterProperty = filterObject.Property(ComplexFilterName);
				if (complexFilterProperty != null)
					result.Add(ParseComplexFilter(complexFilterProperty));

				var userFilterProperty = filterObject.Property(UserFilterName);
				if (userFilterProperty != null)
					result.Add(ParseUserFilter(userFilterProperty));
			}

			return result;
		}

		#endregion

		#region Helper methods

		protected IModelFilterNode ParseSimpleFilter(JProperty filterProperty)
		{
			var result = new ModelFilterNode { Operator = ModelFilterOperators.And };
			var filterObject = filterProperty.Value as JObject;
			if (filterObject == null)
				return result;

			foreach (var filterEntry in filterObject.Properties().Select(p => p.Value).OfType<JObject>())
			{
				string path = null;
				ValueFilterOperators? op = null;
				var negative = false;
				JToken value = null;

				foreach (var property in filterEntry.Properties())
				{
					if ("path".Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
						path = property.Value.TokenValue().TrimSafe();
					else if ("op".Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
						op = ValueFilterOperatorFromToken(property.Value);
					else if ("not".Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
						negative = property.Value.TokenValue().ConvertSafe<bool>();
					else if ("value".Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
						value = property.Value;
				}

				var arrayValue = value as JArray;
				var valValue = value as JValue;
				if (path == null || path.IsNullOrEmpty() || op == null || (arrayValue == null && valValue == null))
					continue;

				if (arrayValue != null && arrayValue.Count == 0)
					continue;

				var parent = result as IModelFilterNode;
				var pathParts = path.Split('.');
				for (var i = 0; i < pathParts.Length - 1; i++)
				{
					var part = pathParts[i];

					var newParent = parent.Items.OfType<IModelFilterNode>().FirstOrDefault(
						m => part.Equals(m.Path, StringComparison.InvariantCulture));
					if (newParent != null)
					{
						parent = newParent;
						continue;
					}

					newParent = new ModelFilterNode { Operator = ModelFilterOperators.And, Path = part };
					parent.AddItem(newParent);
					parent = newParent;
				}
				path = pathParts[pathParts.Length - 1];

				if (valValue != null)
				{
					var strValue = valValue.TokenValue();
					if (!strValue.IsNullOrEmpty())
					{
						parent.AddItem(new ValueFilterNode
						{
							Path = path,
							Operator = op,
							Negative = negative,
							Operand = strValue
						});
					}
					continue;
				}

				var orNode = new ModelFilterNode { Operator = ModelFilterOperators.Or };
				parent.AddItem(orNode);

				foreach (var arrayEntry in arrayValue)
				{
					orNode.AddItem(new ValueFilterNode
					{
						Path = path,
						Operator = op,
						Negative = negative,
						Operand = arrayEntry.TokenValue(arrayEntry is JObject ? "id" : null)
					});
				}
			}

			return result;
		}

		protected IModelFilterNode ParseComplexFilter(JProperty filterProperty)
		{
			return _FilteringService.ParseFilterFromJson(filterProperty.Value.ToStringSafe());
		}

		protected IModelFilterNode ParseUserFilter(JProperty filterProperty)
		{
			var result = new ModelFilterNode { Operator = ModelFilterOperators.And, Path = "CustomProperties" };
			result.AddItem(_FilteringService.ParseFilterFromJson(filterProperty.Value.ToStringSafe()));
			return result;
		}

		protected ValueFilterOperators? ValueFilterOperatorFromToken(JToken token)
		{
			var value = token.TokenValue().TrimSafe();

			var exact = value.ParseEnumSafe<ValueFilterOperators>();
			if (exact != null)
				return exact;

			foreach (var pair in ValueFilterNode.OperatorConversionTable.Where(
					pair => pair.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase)))
				return pair.Key;
			return null;
		}

		#endregion
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using AGO.Core.Attributes.Model;
using AGO.Core.Json;
using AGO.Core.Model;

namespace AGO.Core.Filters
{
	public class FilteringService : AbstractService, IFilteringService
	{
		#region Configuration properties, fields and methods

		public const string SimpleFilterName = "simple";

		public const string ComplexFilterName = "complex";

		public const string UserFilterName = "user";

		protected FilteringServiceOptions _Options = new FilteringServiceOptions();
		public FilteringServiceOptions Options
		{
			get { return _Options; }
			set { _Options = value ?? _Options; }
		}

		protected override void DoSetConfigProperty(string key, string value)
		{
			if ("FormattingCulture".Equals(key))
			{
				_Options.FormattingCulture = CultureInfo.CreateSpecificCulture(value.TrimSafe());
				return;
			}

			_Options.SetMemberValue(key, value);
		}

		protected override string DoGetConfigProperty(string key)
		{
			if ("FormattingCulture".Equals(key))
				return _Options.FormattingCulture != null ? _Options.FormattingCulture.Name : null;

			return _Options.GetMemberValue(key).ToStringSafe();
		}

		#endregion

		#region Properties, fields, constructors

		protected readonly JsonSchema _ModelNodeSchema;

		protected readonly IJsonService _JsonService;

		public FilteringService(IJsonService jsonService)
		{
			if (jsonService == null)
				throw new ArgumentNullException("jsonService");
			_JsonService = jsonService;

			var modelNodeSchemaStream = GetType().Assembly.GetManifestResourceStream(
				string.Format("{0}.{1}", GetType().Namespace, "ModelFilterNodeSchema.json"));
			if (modelNodeSchemaStream == null)
				throw new InvalidOperationException();
			var valueNodeSchemaStream = GetType().Assembly.GetManifestResourceStream(
				string.Format("{0}.{1}", GetType().Namespace, "ValueFilterNodeSchema.json"));
			if (valueNodeSchemaStream == null)
				throw new InvalidOperationException();

			var schemaResolver = new JsonSchemaResolver();
			JsonSchema.Read(_JsonService.CreateReader(
				new StreamReader(valueNodeSchemaStream), true), schemaResolver);
			_ModelNodeSchema = JsonSchema.Read(_JsonService.CreateReader(
				new StreamReader(modelNodeSchemaStream), true), schemaResolver);
		}

		#endregion

		#region Interfaces implementation

		public void ValidateFilter(IModelFilterNode node, Type modelType)
		{
			if (node == null)
				throw new ArgumentNullException("node");
			if (modelType == null)
				throw new ArgumentNullException("modelType");

			try
			{
				ValidateModelFilterNode(node, modelType);
			}
			catch (Exception e)
			{
				throw new FilterValidationException(e);
			}
		}

		public IModelFilterNode ParseFilterFromJson(TextReader reader, Type validateForModelType = null)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");
			IModelFilterNode result;

			try
			{				
				var jsonReader =_JsonService.CreateValidatingReader(reader, _ModelNodeSchema);
				using (jsonReader)
				{
					var node = new ModelFilterNode { Operator = ModelFilterOperators.And };
					result = node;
					var tokens = JToken.ReadFrom(jsonReader) as IEnumerable<JToken>;
					if (tokens == null)
						throw new EmptyFilterDeserializationResultException();
					ProcessModelFilterTokens(tokens, node);
				}
			}
			catch (Exception e)
			{
				throw new FilterJsonException(e);
			}

			if (validateForModelType == null)
				return result;

			ValidateFilter(result, validateForModelType);
			return result;
		}

		public IModelFilterNode ParseFilterFromJson(string str, Type validateForModelType = null)
		{
			if (str.IsNullOrWhiteSpace())
				throw new ArgumentNullException("str");

			return ParseFilterFromJson(new StringReader(str), validateForModelType);
		}

		public IModelFilterNode ParseFilterFromJson(Stream stream, Type validateForModelType = null)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			return ParseFilterFromJson(new StreamReader(stream, true), validateForModelType);			
		}

		public IModelFilterNode ParseUserFilterFromJson(string str, Type validateForModelType = null)
		{
			if (str.IsNullOrWhiteSpace()) return null;

			var result = new ModelFilterNode { Operator = ModelFilterOperators.And, Path = "CustomProperties" };
			result.AddItem(ParseFilterFromJson(str, validateForModelType));

			if (validateForModelType == null)
				return result;

			ValidateFilter(result, validateForModelType);
			return result;
		}

		public IModelFilterNode ParseSimpleFilterFromJson(string str, Type validateForModelType = null)
		{
			if (str.IsNullOrWhiteSpace()) return null;

			return ParseSimpleFilterFromJson(JObject.Parse(str), validateForModelType);
		}

		public IModelFilterNode ParseSimpleFilterFromJson(JObject filterObject, Type validateForModelType = null)
		{
			if (filterObject == null) return null;

			var result = new ModelFilterNode { Operator = ModelFilterOperators.And };

			foreach (var filterEntry in filterObject.Properties().Select(p => p.Value).OfType<JObject>())
			{
				string path = null;
				ValueFilterOperators? op = null;
				ModelFilterOperators? modelOp = null;
				var negative = false;
				JToken value = null;
				IEnumerable<JToken> items = null;

				foreach (var property in filterEntry.Properties())
				{
					if ("path".Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
						path = property.Value.TokenValue().TrimSafe();
					else if ("op".Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
					{
						op = ValueFilterOperatorFromToken(property.Value);
						modelOp = ModelFilterOperatorFromToken(property.Value);
					}
					else if ("not".Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
						negative = property.Value.TokenValue().ConvertSafe<bool>();
					else if ("value".Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
						value = property.Value;
					else if ("items".Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
						items = property.Value;
				}

				if (modelOp != null)
				{
					foreach (var item in (items ?? Enumerable.Empty<JToken>()).OfType<JObject>())
					{
						var subFilter = ParseSimpleFilterFromJson(new JObject { new JProperty("Item", item) });
						if (subFilter != null)
							result.AddItem(subFilter);
					}
					continue;
				}

				var arrayValue = value as JArray;
				var valValue = value as JValue;
				var objValue = value as JObject;

				if (path == null || path.IsNullOrEmpty() || op == null)
					continue;

				var needValue = new ValueFilterNode
				{
					Path = path,
					Operator = op
				}.IsBinary;

				if (needValue && objValue != null)
				{
					var idProperty = objValue.Property("id");
					if (idProperty != null)
						valValue = idProperty.Value as JValue;
				}

				if (needValue && (arrayValue == null || arrayValue.Count == 0) && valValue == null)
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

				if (!needValue)
				{
					parent.AddItem(new ValueFilterNode
					{
						Path = path,
						Operator = op,
						Negative = negative
					});
					continue;
				}

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

			if (validateForModelType == null)
				return result;

			ValidateFilter(result, validateForModelType);
			return result;
		}

		public ICollection<IModelFilterNode> ParseFilterSetFromJson(string str)
		{
			if (str.IsNullOrWhiteSpace())
				return Enumerable.Empty<IModelFilterNode>().ToList();

			return ParseFilterSetFromJson(JObject.Parse(str));
		}

		public ICollection<IModelFilterNode> ParseFilterSetFromJson(JObject filterObject)
		{
			var result = new List<IModelFilterNode>();

			if (filterObject != null)
			{
				var simpleFilterProperty = filterObject.Property(SimpleFilterName);
				var simpleFilter = simpleFilterProperty != null ? ParseSimpleFilterFromJson(simpleFilterProperty.Value as JObject) : null;
				if (simpleFilter != null)
					result.Add(simpleFilter);

				var complexFilterProperty = filterObject.Property(ComplexFilterName);
				var complexFilter = complexFilterProperty != null ? ParseFilterFromJson(complexFilterProperty.Value.ToStringSafe()) : null;
				if (complexFilter != null)
					result.Add(complexFilter);

				var userFilterProperty = filterObject.Property(UserFilterName);
				var userFilter = userFilterProperty != null ? ParseUserFilterFromJson(userFilterProperty.Value.ToStringSafe()) : null;
				if (userFilter != null)
					result.Add(userFilter);
			}

			return result;
		}

		public string GenerateJsonFromFilter(IModelFilterNode node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			try
			{
				var sb = new StringBuilder();
				var writer = new StringWriter(sb);
				_JsonService.CreateSerializer().Serialize(writer, node);
				writer.Flush();

				return sb.ToString();
			}
			catch (Exception e)
			{
				throw new FilterJsonException(e);
			}		
		}

		public DetachedCriteria CompileFilter(IModelFilterNode node, Type modelType)
		{
			if (node == null)
				throw new ArgumentNullException("node");
			if (modelType == null)
				throw new ArgumentNullException("modelType");

			try
			{
				ValidateModelFilterNode(node, modelType);

				var criteria = DetachedCriteria.For(modelType)
				    .SetResultTransformer(new DistinctRootEntityResultTransformer());
				
				var junction = CompileModelFilterNode(node, criteria, modelType, null, new HashSet<string>(), false);
				if (junction != null)
					criteria.Add(junction);

				return criteria;
			}
			catch (Exception e)
			{
				throw new FilterCompilationException(e);
			}
		}

		public IModelFilterNode ConcatFilters(
			IEnumerable<IModelFilterNode> nodes,
			ModelFilterOperators op = ModelFilterOperators.And)
		{
			if (nodes == null)
				throw new ArgumentNullException("nodes");

			try
			{
				var result = new ModelFilterNode { Operator = op };

				foreach (var node in nodes)
				{
					if (node == null)
						continue;
					result.AddItem((IFilterNode) node.Clone());
				}

				return result;			
			}
			catch (Exception e)
			{
				throw new FilterConcatenationException(e);
			}	
		}

		public IModelFilterBuilder<TModel, TModel> Filter<TModel>() 
			where TModel : class, IIdentifiedModel
		{
			return new ModelFilterBuilder<TModel, TModel>(null, null) { Operator = ModelFilterOperators.And };
		}

		#endregion

		#region Template methods

		protected override void DoFinalizeConfig()
		{
			base.DoFinalizeConfig();

			_Options.FormattingCulture = _Options.FormattingCulture ?? CultureInfo.InvariantCulture;
		}

		protected override void DoInitialize()
		{
			base.DoInitialize();

			_JsonService.TryInitialize();
		}

		#endregion

		#region Helper methods

		protected void ValidateModelFilterNode(
			IModelFilterNode node, 
			Type modelType)
		{
			if (!typeof(IIdentifiedModel).IsAssignableFrom(modelType))
				throw new UnexpectedTypeException(modelType);
			
			foreach (var item in node.Items)
			{
				PropertyInfo propertyInfo = null;
				if (!item.Path.IsNullOrWhiteSpace())
				{
					propertyInfo = modelType.GetProperty(item.Path.Trim(), BindingFlags.Public | BindingFlags.Instance);
					if (propertyInfo == null)
						throw new MissingModelPropertyException(item.Path, modelType);

					var notMappedAttribute = propertyInfo.FirstAttribute<NotMappedAttribute>(true);
					if (notMappedAttribute != null)
						throw new NotMappedModelPropertyException(propertyInfo);
				}

				var modelFilterItem = item as IModelFilterNode;
				var valueFilterItem = item as IValueFilterNode;

				if (modelFilterItem != null)
				{
					var innerModelType = modelType;

					if (propertyInfo != null)
					{
						innerModelType = propertyInfo.PropertyType;
						if (typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType) && propertyInfo.PropertyType.IsGenericType)
							innerModelType = propertyInfo.PropertyType.GetGenericArguments()[0];
					}

					ValidateModelFilterNode(modelFilterItem, innerModelType);
				}

				if (valueFilterItem == null)
					continue;

				if (item.Path.IsNullOrEmpty())
					throw new EmptyNodePathException();
				if (propertyInfo == null)
					throw new MissingModelPropertyException(item.Path, modelType);

				ValidateValueFilterNode(valueFilterItem, propertyInfo);		
			}
		}

		protected void ValidateValueFilterNode(IValueFilterNode node, PropertyInfo propertyInfo)
		{
			var propertyType = propertyInfo.PropertyType;
			if (typeof (IEnumerable<IIdentifiedModel>).IsAssignableFrom(propertyType))
				propertyType = propertyType.GetGenericArguments()[0];

			if (!propertyType.IsValue() && !typeof(string).IsAssignableFrom(propertyType) &&
					!typeof(IIdentifiedModel).IsAssignableFrom(propertyType))
				throw new UnexpectedTypeException(propertyType);

			var operandEmpty = node.Operand.IsNullOrWhiteSpace();

			if (node.IsBinary && operandEmpty)
				throw new InvalidFilterOperandException(propertyInfo);

			if (typeof(string).IsAssignableFrom(propertyType))
				ValidateStringValueFilterNode(node, propertyInfo);
			else if (propertyType.IsValueType)
				ValidatePrimitiveValueFilterNode(node, propertyInfo);
			else if (typeof (IIdentifiedModel).IsAssignableFrom(propertyType))
			{
				ValidateModelValueFilterNode(node, propertyInfo);

				var idProperty = propertyType.IsClass 
					? propertyType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance) 
					: null;
				if (idProperty != null)
					propertyType = idProperty.PropertyType;
			}

			if (!operandEmpty && node.Operand.ConvertSafe(propertyType, _Options.FormattingCulture) == null)
				throw new InvalidFilterOperandException(propertyInfo);
		}

		protected void ValidateStringValueFilterNode(IValueFilterNode node, PropertyInfo propertyInfo)
		{
			if (node.IsUnary)
				return;

			var op = node.Operator ?? ValueFilterOperators.Eq;

			if (!node.IsBinary)
				throw new InvalidValueFilterOperatorException(op, propertyInfo);

			if (op == ValueFilterOperators.Gt || op == ValueFilterOperators.Lt ||
					op == ValueFilterOperators.Ge || op == ValueFilterOperators.Le)
				throw new InvalidValueFilterOperatorException(op, propertyInfo);
		}

		protected void ValidatePrimitiveValueFilterNode(IValueFilterNode node, PropertyInfo propertyInfo)
		{
			var op = node.Operator ?? ValueFilterOperators.Eq;

			if (op == ValueFilterOperators.Like)
				throw new InvalidValueFilterOperatorException(op, propertyInfo);
		}

		protected void ValidateModelValueFilterNode(IValueFilterNode node, PropertyInfo propertyInfo)
		{
			var op = node.Operator ?? ValueFilterOperators.Eq;

			if (op != ValueFilterOperators.Eq && op != ValueFilterOperators.Exists)
				throw new InvalidValueFilterOperatorException(op, propertyInfo);
		}

		protected Junction CompileModelFilterNode(
			IModelFilterNode node,
			DetachedCriteria criteria,
			Type modelType, 
			string alias,
			ISet<string> registeredAliases,
			bool negative)
		{
			var op = node.Operator ?? ModelFilterOperators.And;

			var result = op == ModelFilterOperators.And 
				? (Junction) Restrictions.Conjunction() 
				: Restrictions.Disjunction();

			negative = node.Negative ? !negative : negative;

			var hasSubCriterias = false;
			foreach (var item in node.Items)
			{
				PropertyInfo propertyInfo = null;
				if (!item.Path.IsNullOrEmpty())
				{
					propertyInfo = modelType.GetProperty(item.Path, BindingFlags.Public | BindingFlags.Instance);
					if (propertyInfo == null)
						throw new MissingModelPropertyException(item.Path, modelType);
				}
				
				var modelFilterItem = item as IModelFilterNode;
				var valueFilterItem = item as IValueFilterNode;

				if (modelFilterItem != null)
				{
					var newModelType = modelType;
					var newAlias = alias;
					var newPath = alias;

					if (propertyInfo != null)
					{
						newModelType = propertyInfo.PropertyType;
						if (typeof (IEnumerable).IsAssignableFrom(propertyInfo.PropertyType) && propertyInfo.PropertyType.IsGenericType)
							newModelType = propertyInfo.PropertyType.GetGenericArguments()[0];

						if (!newPath.IsNullOrEmpty())
							newPath += '.';
						newPath += item.Path;

						newAlias = newPath.Replace('.', '_');
					}

					var junction = CompileModelFilterNode(modelFilterItem, criteria, newModelType, newAlias, registeredAliases, negative);
					if (junction != null)
					{
						if (!newAlias.IsNullOrWhiteSpace() && !registeredAliases.Contains(newAlias))
						{
							criteria.CreateAlias(newPath, newAlias, JoinType.LeftOuterJoin);
							registeredAliases.Add(newAlias);
						}

						result.Add(junction);
						hasSubCriterias = true;
					}
				}

				if (valueFilterItem == null)
					continue;

				if (propertyInfo == null)
					throw new MissingModelPropertyException(item.Path, modelType);

				result.Add(CompileValueFilterNode(valueFilterItem, criteria,
					propertyInfo, valueFilterItem.Path, alias, registeredAliases, negative));
				hasSubCriterias = true;
			}

			return hasSubCriterias ? result : null;
		}

		protected ICriterion CompileValueFilterNode(
			IValueFilterNode node,
			DetachedCriteria criteria,
			PropertyInfo propertyInfo,
			string path,
			string alias,
			ISet<string> registeredAliases,
			bool negative)
		{
			ICriterion result = null;

			var propertyType = propertyInfo.PropertyType;
			var isTimestamp = propertyInfo.GetCustomAttributes(typeof (TimestampAttribute), false).Length > 0;
			var isModelsCollection = typeof (IEnumerable<IIdentifiedModel>).IsAssignableFrom(propertyType);		
			var realPath = path;
			if (!alias.IsNullOrEmpty())
				realPath = alias + '.' + realPath;
			var realType = propertyType;
			
			negative = node.Negative ? !negative : negative;

			if (isModelsCollection)
			{
				propertyType = propertyType.GetGenericArguments()[0];

				var newAlias = realPath.Replace('.', '_');
				if (!registeredAliases.Contains(realPath))
				{
					criteria.CreateAlias(realPath, newAlias, JoinType.LeftOuterJoin);
					registeredAliases.Add(realPath);
				}
				realPath = newAlias;
			}

			if(typeof(IIdentifiedModel).IsAssignableFrom(propertyType))
			{
				realPath = realPath + ".Id";
				var idProperty = propertyType.IsClass ? propertyType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance) : null;
				if (idProperty != null)
					realType = idProperty.PropertyType;
			}

			var value = node.Operand.ConvertSafe(realType, _Options.FormattingCulture);
			DateTime? dayStart = null;
			DateTime? nextDay = null;
			if (value is DateTime? && !isTimestamp)
			{
				var dateValue = ((DateTime) value).ToLocalTime();
				dayStart = DateTime.SpecifyKind(new DateTime(dateValue.Year, dateValue.Month, dateValue.Day), DateTimeKind.Local);
				nextDay = dayStart.Value.AddDays(1).ToUniversalTime();
				dayStart = dayStart.ToUniversalTime();
			}

			if (node.Operator == ValueFilterOperators.Eq)
			{
				result = dayStart != null
					? Restrictions.Ge(realPath, dayStart.Value) && Restrictions.Lt(realPath, nextDay.Value)
					: Restrictions.Eq(realPath, value);
			}
			else if (node.Operator == ValueFilterOperators.Exists)
			{
				negative = !negative;
				result = Restrictions.IsNull(realPath);
			}
			else if (node.Operator == ValueFilterOperators.Like)
			{
				var str = node.Operand.TrimSafe();
				/*var mode = MatchMode.Exact;
				var prefixed = str.StartsWith("%");
				var suffixed = str.EndsWith("%");
				if (prefixed && suffixed)
					mode = MatchMode.Anywhere;
				else if (prefixed)
					mode = MatchMode.End;
				else if (suffixed)
					mode = MatchMode.Start;
				result = Restrictions.Like(realPath, str.Replace("%", ""), mode);*/
				result = Restrictions.Like(realPath, str.Replace("%", ""), MatchMode.Anywhere);
			}
			else if (node.Operator == ValueFilterOperators.Lt)
			{
				result = dayStart != null
					         ? Restrictions.Lt(realPath, dayStart.Value)
					         : Restrictions.Lt(realPath, value);
			}
			else if (node.Operator == ValueFilterOperators.Gt)
			{
				result = dayStart != null
					         ? Restrictions.Ge(realPath, nextDay.Value)
					         : Restrictions.Gt(realPath, value);
			}
			else if (node.Operator == ValueFilterOperators.Le)
			{
				result = dayStart != null
					         ? Restrictions.Lt(realPath, nextDay.Value)
					         : Restrictions.Le(realPath, value);
			}
			else if (node.Operator == ValueFilterOperators.Ge)
			{
				result = dayStart != null
					         ? Restrictions.Ge(realPath, dayStart.Value)
					         : Restrictions.Ge(realPath, value);
			}

			if (result == null)
				throw new InvalidOperationException("Unexpected operator type");
			
			return negative ? Restrictions.Not(result) : result;
		}

		internal void ProcessModelFilterTokens(IEnumerable<JToken> tokens, ModelFilterNode current)
		{
			foreach (var property in tokens.OfType<JProperty>())
			{
				if ("path".Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
					current.Path = property.Value.TokenValue().TrimSafe();
				else if ("op".Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
					current.Operator = ModelFilterOperatorFromToken(property.Value) ?? ModelFilterOperators.And;
				else if ("not".Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
					current.Negative = property.Value.TokenValue().ConvertSafe<bool>(_Options.FormattingCulture);
				else if (!"items".Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
					continue;

				var array = property.Value as JArray;
				if (array == null)
					continue;

				foreach (var obj in array.OfType<JObject>())
				{
					var opProperty = obj.OfType<JProperty>().FirstOrDefault(
						p => "op".Equals(p.Name, StringComparison.InvariantCultureIgnoreCase));
					if (opProperty == null)
						continue;

					var valueOp = ValueFilterOperatorFromToken(opProperty.Value);
					if (valueOp != null)
					{
						var newValueNode = new ValueFilterNode();
						current.AddItem(newValueNode);
						ProcessValueFilterTokens(obj, newValueNode);
						continue;
					}

					var newModelNode = new ModelFilterNode();
					current.AddItem(newModelNode);
					ProcessModelFilterTokens(obj, newModelNode);
				}
			}
		}

		internal void ProcessValueFilterTokens(IEnumerable<JToken> tokens, ValueFilterNode current)
		{
			foreach (var property in tokens.OfType<JProperty>())
			{
				if ("path".Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
					current.Path = property.Value.TokenValue().TrimSafe();
				else if ("op".Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
					current.Operator = ValueFilterOperatorFromToken(property.Value);
				else if ("not".Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
					current.Negative = property.Value.TokenValue().ConvertSafe<bool>(_Options.FormattingCulture);
				else if ("value".Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
					current.Operand = property.Value.TokenValue();
			}
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

		protected ModelFilterOperators? ModelFilterOperatorFromToken(JToken token)
		{
			var value = token.TokenValue().TrimSafe();

			var exact = value.ParseEnumSafe<ModelFilterOperators>();
			if (exact != null)
				return exact;

			foreach (var pair in ModelFilterNode.OperatorConversionTable.Where(
					pair => pair.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase)))
				return pair.Key;
			return null;
		}

		#endregion
	}
}

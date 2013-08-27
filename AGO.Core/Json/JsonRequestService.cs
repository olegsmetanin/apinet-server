using System;
using System.IO;
using System.Linq;
using AGO.Hibernate;
using AGO.Hibernate.Filters;
using AGO.Hibernate.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AGO.Core.Json
{
	public class JsonRequestService : AbstractService, IJsonRequestService
	{
		#region Constants

		public const string PageName = "page";

		public const string PageSizeName = "pageSize";

		public const string FilterName = "filter";

		public const string SimpleFilterName = "simple";

		public const string ComplexFilterName = "complex";

		public const string UserFilterName = "user";

		public const string ItemsName = "items";

		public const string IdName = "id";

		public const string SortersName = "sorters";

		public const string SortPropertyName = "property";

		public const string SortDescendingName = "descending";

		public const string DontFetchReferencesName = "dontFetchReferences";

		#endregion

		#region Properties, fields, constructors
		
		protected readonly IJsonService _JsonService;

		protected readonly IFilteringService _FilteringService;

		public JsonRequestService(IJsonService jsonService, IFilteringService filteringService)
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

		public IJsonModelsRequest ParseModelsRequest(JsonReader reader, int defaultPageSize, int maxPageSize)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");
			if (defaultPageSize <= 0)
				throw new ArgumentException("defaultPageSize <= 0");
			if (maxPageSize <= 0)
				throw new ArgumentException("maxPageSize <= 0");

			try
			{
				var result = new JsonModelsRequest();
				ParseRequest(reader, result);

				var filterProperty = result.Body.Property(FilterName);
				var filterObject = filterProperty != null ? filterProperty.Value as JObject : null;

				if (filterObject != null)
				{
					var simpleFilterProperty = filterObject.Property(SimpleFilterName);
					if (simpleFilterProperty != null)
						result.Filters.Add(ParseSimpleFilter(simpleFilterProperty));

					var complexFilterProperty = filterObject.Property(ComplexFilterName);
					if (complexFilterProperty != null)
						result.Filters.Add(ParseComplexFilter(complexFilterProperty));

					var userFilterProperty = filterObject.Property(UserFilterName);
					if (userFilterProperty != null)
						result.Filters.Add(ParseUserFilter(userFilterProperty));
				}

				var sortersProperty = result.Body.Property(SortersName);
				var sortersArray = sortersProperty != null ? sortersProperty.Value as JArray : null;
				if (sortersArray != null)
				{
					foreach (var sorter in sortersArray.OfType<JObject>())
					{
						result.Sorters.Add(new SortInfo
						{
							Property = sorter.TokenValue(SortPropertyName).TrimSafe(),
							Descending = sorter.TokenValue(SortDescendingName).ConvertSafe<bool>()
						});
					}
				}

				var page = 0;
				var pageProperty = result.Body.Property(PageName);
				if (pageProperty != null)
					page = pageProperty.TokenValue().ConvertSafe<int>();
				if (page < 0)
					result.Page = 0;

				var pageSize = defaultPageSize;
				var pageSizeProperty = result.Body.Property(PageSizeName);
				if (pageSizeProperty != null)
					pageSize = pageSizeProperty.TokenValue().ConvertSafe<int>();
				if (pageSize <= 0)
					pageSize = defaultPageSize;
				if (pageSize > maxPageSize)
					result.PageSize = maxPageSize;

				return result;
			}
			catch (Exception e)
			{
				throw new JsonRequestException(e);
			}
		}

		public IJsonModelsRequest ParseModelsRequest(TextReader reader, int defaultPageSize, int maxPageSize)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			var jsonReader = _JsonService.CreateReader(reader);
			using (jsonReader)
				return ParseModelsRequest(jsonReader, defaultPageSize, maxPageSize);	
		}

		public IJsonModelsRequest ParseModelsRequest(string str, int defaultPageSize, int maxPageSize)
		{
			if (str.IsNullOrWhiteSpace())
				throw new ArgumentNullException("str");

			return ParseModelsRequest(new StringReader(str), defaultPageSize, maxPageSize);
		}

		public IJsonModelsRequest ParseModelsRequest(Stream stream, int defaultPageSize, int maxPageSize)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			return ParseModelsRequest(new StreamReader(stream, true), defaultPageSize, maxPageSize);
		}

		public IJsonModelRequest<TIdType> ParseModelRequest<TIdType>(JsonReader reader) 
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			try
			{
				var result = new JsonModelRequest<TIdType>();
				ParseRequest(reader, result);

				var idProperty = result.Body.Property(IdName);
				var id = idProperty != null ? idProperty.TokenValue().TrimSafe() : null;
				if (id.IsNullOrEmpty())
					throw new JsonRequestBodyMissesPropertyException(IdName);

				result.Id = id.ConvertSafe<TIdType>();
				object idObj = result.Id;
				if (idObj == null)
					throw new JsonRequestBodyMissesPropertyException(IdName);

				return result;
			}
			catch (Exception e)
			{
				throw new JsonRequestException(e);
			}
		}

		public IJsonModelRequest<TIdType> ParseModelRequest<TIdType>(TextReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			var jsonReader = _JsonService.CreateReader(reader);
			using (jsonReader)
				return ParseModelRequest<TIdType>(jsonReader);			
		}

		public IJsonModelRequest<TIdType> ParseModelRequest<TIdType>(string str)
		{
			if (str.IsNullOrWhiteSpace())
				throw new ArgumentNullException("str");

			return ParseModelRequest<TIdType>(new StringReader(str));
		}

		public IJsonModelRequest<TIdType> ParseModelRequest<TIdType>(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			return ParseModelRequest<TIdType>(new StreamReader(stream, true));
		}

		#endregion

		#region Template methods

		protected override void DoInitialize()
		{
			base.DoInitialize();

			var initializable = _JsonService as IInitializable;
			if (initializable != null)
				initializable.Initialize();

			initializable = _FilteringService as IInitializable;
			if (initializable != null)
				initializable.Initialize();
		}

		#endregion

		#region Helper method

		internal void ParseRequest(JsonReader reader, JsonRequest request)
		{
			request.Body = JToken.ReadFrom(reader) as JObject;

			if (request.Body == null)
				throw new JsonRequestBodyEmptyException();

			request.DontFetchReferences = request.Body.TokenValue(
				DontFetchReferencesName).ConvertSafe<bool>();
		}

		protected IModelFilterNode ParseSimpleFilter(JProperty filterProperty)
		{
			var result = new ModelFilterNode {Operator = ModelFilterOperators.And};
			var filterObject = filterProperty.Value as JObject;
			if (filterObject == null)
				return result;
			
			foreach(var filterEntry in filterObject.Properties().Select(p => p.Value).OfType<JObject>())
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
			var result = new ModelFilterNode {Operator = ModelFilterOperators.And, Path = "CustomProperties"};
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
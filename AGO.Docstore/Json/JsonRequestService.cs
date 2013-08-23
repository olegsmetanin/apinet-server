using System;
using System.IO;
using System.Linq;
using AGO.Hibernate;
using AGO.Hibernate.Filters;
using AGO.Hibernate.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AGO.Docstore.Json
{
	public class JsonRequestService : AbstractService, IJsonRequestService
	{
		#region Constants

		public const string PageName = "page";

		public const string PageSizeName = "pageSize";

		public const string FilterName = "filter";

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

				if (filterObject != null && filterObject.Property(ItemsName) != null)
					result.Filters.Add(_FilteringService.ParseFilterFromJson(filterObject.ToString()));
				if (filterObject != null && filterObject.Property(ItemsName) == null)
				{
					foreach (var subFilterObject in filterObject.Properties().Select(f => f.Value).OfType<JObject>())
						result.Filters.Add(_FilteringService.ParseFilterFromJson(subFilterObject.ToString()));
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

		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using AGO.Docstore.Model;
using AGO.Hibernate;
using AGO.Hibernate.Filters;
using AGO.Hibernate.Json;
using Newtonsoft.Json.Linq;

namespace AGO.WebApiApp.Controllers
{
	public class ModelsApiController : Controller
	{
		#region Constants
		
		public const string SuccessResult = "success";

		public const string ErrorResult = "error";

		public const string PageName = "page";

		public const string PageSizeName = "pageSize";

		public const int DefaultPageSize = 15;

		public const int MaxPageSize = 100;

		public const string ActionName = "action";

		public const string ModelTypeName = "modelType";

		public const string FilterName = "filter";

		public const string IdName = "id";

		public const string SortersName = "sorters";

		public const string SortPropertyName = "property";

		public const string SortDescendingName = "descending";
		
		public const string GetModelsActionName = "getModels";

		public const string GetModelActionName = "getModel";

		public const string DontFetchReferencesName = "dontFetchReferences";

		#endregion

		#region Controller actions

		public ActionResult Index()
		{
			try
			{
				if (HttpContext.Request.ContentLength == 0)
					throw new Exception("Empty request body");
				
				var jsonService = DependencyResolver.Current.GetService<IJsonService>();
					
				var jsonReader = jsonService.CreateReader(new StreamReader(HttpContext.Request.InputStream, true));
				JObject body;
				using (jsonReader)
					body = JToken.ReadFrom(jsonReader) as JObject;

				if (body == null)
					throw new Exception("Empty request body");

				var actionProperty = body.Property(ActionName);
				var action = actionProperty != null ? actionProperty.TokenValue().TrimSafe() : null;
				if (action.IsNullOrEmpty())
					throw new Exception("Request body missing action");

				if (GetModelsActionName.Equals(action, StringComparison.InvariantCultureIgnoreCase))
					return GetModels(body);
				if (GetModelActionName.Equals(action, StringComparison.InvariantCultureIgnoreCase))
					return GetModel(body);

				throw new Exception(string.Format("Unknown action: {0}", action));
			}
			catch (Exception e)
			{
				return Json(new { result = ErrorResult, message = e.Message });
			}
		}

		#endregion

		#region Helper methods

		protected ActionResult GetModel(JObject body)
		{
			var options = new FilteringOptions
			{
				ModelType = ModelTypeFromRequestBody(body),
				Take = 1,
				FetchStrategy = FetchStrategy.FetchRootReferences
			};

			var idProperty = body.Property(IdName);
			var id = idProperty != null ? idProperty.TokenValue().TrimSafe() : null;
			if (id.IsNullOrEmpty())
				throw new Exception("Id not specified");

			var dontFetchReferencesProperty = body.Property(DontFetchReferencesName);
			var dontFetchReferences = dontFetchReferencesProperty != null && 
				dontFetchReferencesProperty.TokenValue().ConvertSafe<bool>();

			IDocstoreModel result;

			if (dontFetchReferences)
			{
				var modelIdProperty = options.ModelType.GetProperty("Id");

				var crudDao = DependencyResolver.Current.GetService<ICrudDao>();
				result = crudDao.Get<IDocstoreModel>(modelIdProperty != null ? 
					id.ConvertSafe(modelIdProperty.PropertyType) : id, false, options.ModelType);
			}
			else
			{
				var filteringDao = DependencyResolver.Current.GetService<IFilteringDao>();
				var filteringService = DependencyResolver.Current.GetService<IFilteringService>();
				var json = @"
				{
					op: '&&',
					items: [ { path: 'Id', op: '=', value: '" + id + @"' } ]
				}";

				result = filteringDao.List<IDocstoreModel>(
					new[] {filteringService.ParseFilterFromJson(json)}, options).FirstOrDefault();
			}

			return Content(Serialize(result), "application/json", Encoding.UTF8);
		}

		protected ActionResult GetModels(JObject body)
		{
			var options = new FilteringOptions { ModelType = ModelTypeFromRequestBody(body) };
		
			var filters = new List<IModelFilterNode>();
			var filteringService = DependencyResolver.Current.GetService<IFilteringService>();
			var filterProperty = body.Property(FilterName);
			if (filterProperty != null)
				filters.Add(filteringService.ParseFilterFromJson(filterProperty.Value.ToString()));

			var sortersProperty = body.Property(SortersName);
			var sortersArray = sortersProperty != null ? sortersProperty.Value as JArray : null;
			if (sortersArray != null)
			{
				foreach (var sorter in sortersArray.OfType<JObject>())
				{
					options.Sorters.Add(new SortInfo
					{
						Property = sorter.TokenValue(SortPropertyName).TrimSafe(),
						Descending = sorter.TokenValue(SortDescendingName).ConvertSafe<bool>()
					});
				}
			}

			var page = 0;
			var pageProperty = body.Property(PageName);
			if (pageProperty != null)
				page = pageProperty.TokenValue().ConvertSafe<int>();
			if (page < 0)
				page = 0;

			var pageSize = DefaultPageSize;
			var pageSizeProperty = body.Property(PageSizeName);
			if (pageSizeProperty != null)
				pageSize = pageSizeProperty.TokenValue().ConvertSafe<int>();
			if (pageSize <= 0)
				pageSize = DefaultPageSize;
			if (pageSize > MaxPageSize)
				pageSize = MaxPageSize;

			options.Skip = page*pageSize;
			options.Take = pageSize;

			var filteringDao = DependencyResolver.Current.GetService<IFilteringDao>();

			var totalRows = filteringDao.RowCount<IDocstoreModel>(filters, options.ModelType);
			var modelsList = filteringDao.List<IDocstoreModel>(filters, options);

			var result = new
			{
				totalRowsCount = totalRows,
				rows = modelsList
			};

			return Content(Serialize(result), "application/json", Encoding.UTF8);
		}

		protected Type ModelTypeFromRequestBody(JObject body)
		{
			var modelTypeProperty = body.Property(ModelTypeName);
			var modelTypeName = modelTypeProperty != null ? modelTypeProperty.TokenValue().TrimSafe() : null;
			var modelType = !modelTypeName.IsNullOrEmpty()
				? typeof(IDocstoreModel).Assembly.GetType(modelTypeName, false)
				: null;
			if (!typeof(IDocstoreModel).IsAssignableFrom(modelType))
				throw new Exception("Model type not specified or invalid");
			return modelType;
		}

		protected string Serialize(object obj)
		{
			var result = "null";

			if (obj != null)
			{
				var jsonService = DependencyResolver.Current.GetService<IJsonService>();

				var stringBuilder = new StringBuilder();
				jsonService.CreateSerializer().Serialize(
					jsonService.CreateWriter(new StringWriter(stringBuilder), true), obj);
				result = stringBuilder.ToString();
			}

			return result;
		}

		#endregion
	}
}

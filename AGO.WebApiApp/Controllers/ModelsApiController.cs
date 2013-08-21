using System;
using System.Collections.Generic;
using System.IO;
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

		public const string PagePropertyName = "page";

		public const string PageSizePropertyName = "pageSize";

		public const int DefaultPageSize = 15;

		public const int MaxPageSize = 100;

		public const string ActionPropertyName = "action";

		public const string ModelTypePropertyName = "modelType";

		public const string FilterPropertyName = "filter";

		public const string SortersPropertyName = "sorters";

		public const string SortFieldPropertyName = "field";

		public const string SortDirectionPropertyName = "direction";

		public const string GetModelsActionName = "getModels";

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

				var actionProperty = body.Property(ActionPropertyName);
				var action = actionProperty != null ? actionProperty.TokenValue().TrimSafe() : null;
				if (action.IsNullOrEmpty())
					throw new Exception("Request body missing action");

				if (GetModelsActionName.Equals(action, StringComparison.InvariantCultureIgnoreCase))
					return GetModels(body, jsonService);
				
				throw new Exception(string.Format("Unknown action: {0}", action));
			}
			catch (Exception e)
			{
				return Json(new { result = ErrorResult, message = e.Message });
			}
		}

		#endregion

		#region Helper methods

		public ActionResult GetModels(JObject body, IJsonService jsonService)
		{
			var page = 0;
			var pageSize = DefaultPageSize;
			var filters = new List<IModelFilterNode>();
			string orderBy = null;
			var descending = false;

			var modelTypeProperty = body.Property(ModelTypePropertyName);
			var modelTypeName = modelTypeProperty != null ? modelTypeProperty.TokenValue().TrimSafe() : null;		
			var modelType = !modelTypeName.IsNullOrEmpty() 
				? typeof (IDocstoreModel).Assembly.GetType(modelTypeName, false)
				: null;
			if (!typeof (IDocstoreModel).IsAssignableFrom(modelType))
				throw new Exception("Model type not specified or invalid");

			var filteringService = DependencyResolver.Current.GetService<IFilteringService>();
			var filterProperty = body.Property(FilterPropertyName);
			if (filterProperty != null)
				filters.Add(filteringService.ParseFilterFromJson(filterProperty.Value.ToString()));

			var sortersProperty = body.Property(SortersPropertyName);
			if (sortersProperty != null)
			{
				var sortersArray = sortersProperty.Value as JArray;
				if (sortersArray != null && sortersArray.HasValues)
				{
					var firstSorter = sortersArray[0] as JObject;
					if (firstSorter != null)
					{
						orderBy = firstSorter.TokenValue(SortFieldPropertyName).TrimSafe();
						descending = firstSorter.TokenValue(SortDirectionPropertyName).TrimSafe().Equals(
							"desc", StringComparison.InvariantCultureIgnoreCase);
					}
				}
			}

			var pageProperty = body.Property(PagePropertyName);
			if (pageProperty != null)
				page = pageProperty.TokenValue().ConvertSafe<int>();

			var pageSizeProperty = body.Property(PageSizePropertyName);
			if (pageSizeProperty != null)
				pageSize = pageSizeProperty.TokenValue().ConvertSafe<int>();

			if (page < 0)
				page = 0;

			if (pageSize <= 0)
				pageSize = DefaultPageSize;
			if (pageSize > MaxPageSize)
				pageSize = MaxPageSize;

			var filteringDao = DependencyResolver.Current.GetService<IFilteringDao>();

			var totalRows = filteringDao.RowCount<IDocstoreModel>(filters, modelType);
			var modelsList = filteringDao.List<IDocstoreModel>(filters, orderBy, !descending, page * pageSize, pageSize, modelType);

			var result = new
			{
				totalRowsCount = totalRows,
				rows = modelsList
			};

			return Content(Serialize(result), "application/json", Encoding.UTF8);
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

using AGO.Core;
using AGO.Core.Controllers;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Tasks.Model.Dictionary;
using Newtonsoft.Json;

namespace AGO.Tasks.Controllers
{
    /// <summary>
    /// Контроллер справочников модуля задач
    /// </summary>
    public class DictionaryController: AbstractController
    {
        public DictionaryController(
            IJsonService jsonService, 
            IFilteringService filteringService, 
            IJsonRequestService jsonRequestService, 
            ICrudDao crudDao, 
            IFilteringDao filteringDao, 
            ISessionProvider sessionProvider)
            : base(jsonService, filteringService, jsonRequestService, crudDao, filteringDao, sessionProvider)
        {
        }

		public void GetTaskTypes(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			_JsonService.CreateSerializer().Serialize(output, new
			{
				totalRowsCount = _FilteringDao.RowCount<TaskTypeModel>(request.Filters),
				rows = _FilteringDao.List<TaskTypeModel>(request.Filters, OptionsFromRequest(request))
			});
		}
    }
}
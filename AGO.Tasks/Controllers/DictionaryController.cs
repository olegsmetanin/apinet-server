using AGO.Core;
using AGO.Core.Controllers;
using AGO.Core.Filters;
using AGO.Core.Json;

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
    }
}
using AGO.Core;
using AGO.Core.Controllers;
using AGO.Core.Filters;
using AGO.Core.Json;

namespace AGO.Tasks.Controllers
{
    /// <summary>
    /// Контроллер работы с задачами модуля задач
    /// </summary>
    public class TasksController: AbstractController
    {
        public TasksController(
            IJsonService jsonService, 
            IFilteringService filteringService, 
            IJsonRequestService jsonRequestService, 
            ICrudDao crudDao, 
            IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			AuthController authController) 
            : base(jsonService, filteringService, jsonRequestService, crudDao, filteringDao, sessionProvider, authController)
        {
        }
    }
}
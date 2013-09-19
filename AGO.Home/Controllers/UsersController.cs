using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Modules.Attributes;
using Newtonsoft.Json;

namespace AGO.Home.Controllers
{
	public class UsersController : AbstractController
	{
		#region Constants

		public const string ProjectProperty = "project";

		#endregion

		#region Properties, fields, constructors

		public UsersController(
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

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization]
		public void GetRole(JsonReader input, JsonWriter output)
		{
			_JsonService.CreateSerializer().Serialize(output, string.Empty);
		}

		#endregion
	}
}
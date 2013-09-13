using System;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Modules.Attributes;
using Newtonsoft.Json;

namespace AGO.Home.Controllers
{
	public class ConfigController : AbstractController
	{
		#region Constants

		public const string ProjectProperty = "project";

		#endregion

		#region Properties, fields, constructors

		protected readonly AuthController _AuthController;

		public ConfigController(
			IJsonService jsonService,
			IFilteringService filteringService,
			IJsonRequestService jsonRequestService,
			ICrudDao crudDao,
			IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			AuthController authController)
			: base(jsonService, filteringService, jsonRequestService, crudDao, filteringDao, sessionProvider)
		{
			if (authController == null)
				throw new ArgumentNullException("authController");
			_AuthController = authController;
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization]
		public void GetConfig(JsonReader input, JsonWriter output)
		{
			_JsonService.CreateSerializer().Serialize(output, new { });
		}

		#endregion
	}
}
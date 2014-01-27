using AGO.Core.Json;
using AGO.Core.Filters;
using AGO.Core.Localization;
using AGO.Core.Model.Processing;

namespace AGO.Core.Controllers
{
	//TODO: Вынести сюда общую логику связанную с тегами
	public abstract class AbstractTagsAwareController : AbstractController
	{
		#region Properties, fields, constructors

		protected AbstractTagsAwareController(
			IJsonService jsonService,
			IFilteringService filteringService,
			ICrudDao crudDao,
			IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			ILocalizationService localizationService,
			IModelProcessingService modelProcessingService,
			AuthController authController)
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, localizationService, modelProcessingService, authController)
		{
		}

		#endregion

		#region Helper methods



		#endregion
	}
}
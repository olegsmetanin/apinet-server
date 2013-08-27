using AGO.Core.Json;
using AGO.Core.Model.Projects;
using AGO.Hibernate;
using AGO.Hibernate.Filters;
using AGO.Hibernate.Json;
using AGO.Hibernate.Modules.Attributes;
using Newtonsoft.Json;

namespace AGO.Core.Controllers
{
	public class ProjectsController : AbstractController
	{
		#region Properties, fields, constructors

		public ProjectsController(
			IJsonService jsonService,
			IFilteringService filteringService,
			IJsonRequestService jsonRequestService,
			ICrudDao crudDao,
			IFilteringDao filteringDao)
			: base(jsonService, filteringService, jsonRequestService, crudDao, filteringDao)
		{
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint]
		public void GetProjects(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			_JsonService.CreateSerializer().Serialize(output, new
			{
				totalRowsCount = _FilteringDao.RowCount<ProjectModel>(request.Filters),
				rows = _FilteringDao.List<ProjectModel>(request.Filters, OptionsFromRequest(request))
			});
		}

		#endregion
	}
}
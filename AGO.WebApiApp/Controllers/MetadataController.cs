using System.IO;
using System.Text;
using System.Web.Mvc;
using AGO.Hibernate;
using AGO.Hibernate.Json;

namespace AGO.WebApiApp.Controllers
{
	public class MetadataController : Controller
	{
		public ActionResult AllModelsMetadata()
		{
			var sessionProvider = DependencyResolver.Current.GetService<ISessionProvider>();
			var jsonService = DependencyResolver.Current.GetService<IJsonService>();

			var metadata = sessionProvider.AllModelsMetadata;
			
			var stringBuilder = new StringBuilder();
			jsonService.CreateSerializer().Serialize(
				jsonService.CreateWriter(new StringWriter(stringBuilder), true), metadata);

			return new ContentResult
			{
				Content = stringBuilder.ToString(),
				ContentEncoding = Encoding.UTF8,
				ContentType = "application/json"
			};
		}
	}
}

using System.Web.Mvc;

namespace AGO.WebApiApp.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}
	}
}
using System.Web.Mvc;
using AGO.WebApiApp.App_Start;

namespace AGO.WebApiApp.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return WebApplication.DevMode == DevMode.Dev
				? View("IndexDev")
				: View("IndexProd");
		}
	}
}

using System.Web.Mvc;
using AGO.WebApiApp.Application;

namespace AGO.WebApiApp.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			var model = new SysConfigData("src", "home");

			return WebApplication.DevMode == DevMode.Dev
				? View("IndexDev", model)
				: View("IndexProd", model);
		}

		public ActionResult Project(string project)
		{
			var model = new SysConfigData("../../src", "../../", project, "tasks"); //TODO get module from project

			return WebApplication.DevMode == DevMode.Dev
				? View("IndexDev", model)
				: View("IndexProd", model);
		}
	}
}

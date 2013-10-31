using System.Web.Mvc;
using AGO.System.Controllers;
using AGO.WebApiApp.Application;

namespace AGO.WebApiApp.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			var usersController = DependencyResolver.Current.GetService<UsersController>();
			usersController.SetLocale(null, HttpContext.Request.UserLanguages);

			var model = new SysConfigData("src", "home");

			return WebApplication.DevMode == DevMode.Dev
				? View("IndexDev", model)
				: View("IndexProd", model);
		}

		public ActionResult Project(string project)
		{
			var usersController = DependencyResolver.Current.GetService<UsersController>();
			usersController.SetLocale(null, HttpContext.Request.UserLanguages);

			var model = new SysConfigData("../../src", "../../", project, "tasks"); //TODO get module from project

			return WebApplication.DevMode == DevMode.Dev
				? View("IndexDev", model)
				: View("IndexProd", model);
		}
	}
}

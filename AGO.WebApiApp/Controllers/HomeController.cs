using System.Web.Mvc;
using AGO.Core.Controllers;
using AGO.WebApiApp.Application;

namespace AGO.WebApiApp.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			var usersController = DependencyResolver.Current.GetService<UsersController>();
			usersController.SetLocale(null, HttpContext.Request.UserLanguages);

			return WebApplication.DevMode == DevMode.Dev ? View("IndexDev") : View("IndexProd");
		}
	}
}

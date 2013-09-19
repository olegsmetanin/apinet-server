using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages.Scope;
using AGO.Core;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Execution;
using AGO.Core.Json;

namespace AGO.WebApiApp.Controllers
{
	public class ApiController : Controller
	{
		public ActionResult Dispatch()
		{
			using (ScopeStorage.CreateTransientScope(new Dictionary<object, object>()))
			{
				try
				{
					var serviceType = RouteData.Values["serviceType"] as Type;
					if (serviceType == null)
						throw new Exception("serviceType is null");

					var method = RouteData.Values["method"] as MethodInfo;
					if (method == null)
						throw new Exception("method is null");

					var service = DependencyResolver.Current.GetService(serviceType);
					var initializable = service as IInitializable;
					if (initializable != null)
						initializable.Initialize();

					var requireAuthorizationAttribute = method.FirstAttribute<RequireAuthorizationAttribute>(false);
					if (requireAuthorizationAttribute != null)
					{
						var authController = DependencyResolver.Current.GetService<AuthController>();
						if (authController == null)
							throw new NotAuthorizedException();

						if (!authController.IsAuthenticated)
							throw new NotAuthorizedException();

						if (requireAuthorizationAttribute.RequireAdmin && !authController.IsAdmin)
							throw new AccessForbiddenException();
					}

					var executor = DependencyResolver.Current.GetService<IActionExecutor>();
					var result = executor.Execute(service, method);

					DependencyResolver.Current.GetService<ISessionProvider>().CurrentSession.Flush();

					var jsonService = DependencyResolver.Current.GetService<IJsonService>();
					var stringBuilder = new StringBuilder();
					var outputWriter = jsonService.CreateWriter(new StringWriter(stringBuilder), true);
					jsonService.CreateSerializer().Serialize(outputWriter, result);

					return Content(stringBuilder.ToString(), "application/json", Encoding.UTF8);
				}
				catch (NotAuthorizedException e)
				{
					HttpContext.Response.StatusCode = 401;
					return Json(new {message = e.Message}, JsonRequestBehavior.AllowGet);
				}
				catch (AccessForbiddenException e)
				{
					HttpContext.Response.StatusCode = 403;
					return Json(new {message = e.Message}, JsonRequestBehavior.AllowGet);
				}
				catch (HttpException e)
				{
					HttpContext.Response.StatusCode = e.GetHttpCode();
					return Json(new {error = e.InnerException != null ? e.InnerException.Message : e.Message},
						JsonRequestBehavior.AllowGet);
				}
				catch (Exception e)
				{
					HttpContext.Response.StatusCode = 500;
					return Json(new {message = e.Message}, JsonRequestBehavior.AllowGet);
				}
			}
		}
	}
}
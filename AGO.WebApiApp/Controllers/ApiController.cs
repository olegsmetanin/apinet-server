using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AGO.Core;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Json;
using Newtonsoft.Json;

namespace AGO.WebApiApp.Controllers
{
	public class ApiController : Controller
	{
		public ActionResult Dispatch()
		{			
			try
			{
				var serviceType = RouteData.Values["serviceType"] as Type;
				if (serviceType == null)
					throw new Exception("serviceType is null");

				var method = RouteData.Values["method"] as MethodInfo;
				if (method == null)
					throw new Exception("method is null");

				var jsonService = DependencyResolver.Current.GetService<IJsonService>();
				JsonReader inputReader = null;
				JsonWriter outputWriter = null;
				var stringBuilder = new StringBuilder();

				var parameters = new List<object>();
				foreach (var parameterInfo in method.GetParameters())
				{
					if (typeof (JsonReader).IsAssignableFrom(parameterInfo.ParameterType))
					{
						inputReader = inputReader ??
						              jsonService.CreateReader(new StreamReader(HttpContext.Request.InputStream, true));
						parameters.Add(inputReader);
						continue;
					}
					if (typeof (JsonWriter).IsAssignableFrom(parameterInfo.ParameterType))
					{
						outputWriter = outputWriter ??
						               jsonService.CreateWriter(new StringWriter(stringBuilder), true);
						parameters.Add(outputWriter);
						continue;
					}
					parameters.Add(null);
				}

				if (inputReader == null || outputWriter == null)
					throw new Exception("Invalid service method signature");

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

					if(!authController.IsAuthenticated)
						throw new NotAuthorizedException();

					if (requireAuthorizationAttribute.RequireAdmin && !authController.IsAdmin)
						throw new AccessForbiddenException();
				}

				try
				{
					method.Invoke(DependencyResolver.Current.GetService(serviceType), parameters.ToArray());
					DependencyResolver.Current.GetService<ISessionProvider>().CloseCurrentSession();
				}
				catch (TargetInvocationException e)
				{
					throw e.InnerException;
				}

				outputWriter.Flush();
				return Content(stringBuilder.ToString(), "application/json", Encoding.UTF8);
			}			
			catch (NotAuthorizedException e)
			{
				HttpContext.Response.StatusCode = 401;
				return Json(new { message = e.Message }, JsonRequestBehavior.AllowGet);
			}
			catch (AccessForbiddenException e)
			{
				HttpContext.Response.StatusCode = 403;
				return Json(new { message = e.Message }, JsonRequestBehavior.AllowGet);
			}
			catch (HttpException e)
			{
				HttpContext.Response.StatusCode = e.GetHttpCode();
				return Json(new { error = e.InnerException != null ? e.InnerException.Message : e.Message }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception e)
			{
				HttpContext.Response.StatusCode = 500;
				return Json(new { message = e.Message }, JsonRequestBehavior.AllowGet);
			}
		}
	}
}
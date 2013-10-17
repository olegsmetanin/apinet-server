using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages.Scope;
using AGO.Core;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Execution;
using AGO.Core.Json;
using AGO.Core.Localization;
using Common.Logging;

namespace AGO.WebApiApp.Controllers
{
	public class ApiController : Controller
	{
		#region Constants

		internal const string CurrentCultureCookie = "currentLocale";
		 
		#endregion

		public ActionResult Dispatch()
		{
			var cultureCookie = HttpContext.Request.Cookies.Get(CurrentCultureCookie);
			var cultureName = cultureCookie != null ? cultureCookie.Value.TrimSafe() : null;
			var cultureInfo = !string.IsNullOrEmpty(cultureName) ? CultureInfo.GetCultureInfo(cultureName) : null;
			if (cultureInfo != null && !cultureInfo.Equals(CultureInfo.CurrentUICulture))
				Thread.CurrentThread.CurrentUICulture = cultureInfo;

			using (ScopeStorage.CreateTransientScope(new Dictionary<object, object>()))
			{
				var logged = false;
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
							throw new NotAuthenticatedException();

						if (!authController.IsAuthenticated())
							throw new NotAuthenticatedException();

						if (requireAuthorizationAttribute.RequireAdmin && !authController.IsAdmin())
							throw new AccessForbiddenException();
					}

					var executor = DependencyResolver.Current.GetService<IActionExecutor>();
					object result;
					try
					{
						result = executor.Execute(service, method);
					}
					catch (Exception)
					{
						logged = true;
						throw;
					}

					var jsonService = DependencyResolver.Current.GetService<IJsonService>();
					var stringBuilder = new StringBuilder();
					var outputWriter = jsonService.CreateWriter(new StringWriter(stringBuilder), true);
					jsonService.CreateSerializer().Serialize(outputWriter, result);

					return Content(stringBuilder.ToString(), "application/json", Encoding.UTF8);
				}
				catch (Exception e)
				{
					//may be logged in IActionExecutor with more detailed info
					if (!logged)
						LogException(e);

					try
					{
						HttpContext.Response.TrySkipIisCustomErrors = true;

						HttpContext.Response.StatusCode = 500;
						if (e is NotAuthenticatedException)
							HttpContext.Response.StatusCode = 401;
						if (e is AccessForbiddenException)
							HttpContext.Response.StatusCode = 403;
					
						var httpException = e as HttpException;
						if (httpException != null)
							HttpContext.Response.StatusCode = httpException.GetHttpCode();

						var message = new StringBuilder();
						var localizationService = DependencyResolver.Current.GetService<ILocalizationService>();

						message.Append(localizationService.MessageForException(e));
						if (message.Length == 0)
							message.Append(localizationService.MessageForException(new ExceptionDetailsHidden()));
						else
						{
							var subMessage = e.InnerException != null
							                 	? localizationService.MessageForException(e.InnerException)
							                 	: null;
							if (!subMessage.IsNullOrEmpty())
								message.Append(string.Format(" ({0})", subMessage.FirstCharToLower()));
						}

						return Json(new { message = message.ToString() });
					}
					catch (Exception ex)
					{
						LogException(ex);//fatal, but we need cause of exception in log
						throw;
					}
				}
			}
		}

		private void LogException(Exception e)
		{
			if (e is AbstractApplicationException)
				LogManager.GetLogger(GetType()).Info(e.GetBaseException().Message, e);
			else
				LogManager.GetLogger(GetType()).Error(e.GetBaseException().Message, e);
		}
	}
}
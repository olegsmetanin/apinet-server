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
using AGO.Core.Controllers.Security;
using AGO.Core.Execution;
using AGO.Core.Json;
using AGO.Core.Localization;
using Common.Logging;
using UsersController = AGO.Core.Controllers.Security.UsersController;

namespace AGO.WebApiApp.Controllers
{
	//TODO use BaseMvcController
	public class ApiController : Controller
	{
		private const string OptionsHttpMethod = "OPTIONS";
		public ActionResult Dispatch()
		{
		    var agoRequestedWith = Request.Headers["X-AGO-Requested-With"];
            var origin = Request.Headers["Origin"];
		    if (OptionsHttpMethod.Equals(Request.HttpMethod, StringComparison.InvariantCultureIgnoreCase))
		    {
                Response.AddHeader("Access-Control-Allow-Origin", origin);
		        return Content(string.Empty);
		    }

		    if (agoRequestedWith == "easyXDM")
		    {
		        //there Origin header is our api, so, we can not use Origin and return wildcard (that is bad, i know)
		        Response.AddHeader("Access-Control-Allow-Origin", "*");
		    }
		    else
		    {
		        //this is not easyXDM request (off or file upload request) and we can use Origin safely
                Response.AddHeader("Access-Control-Allow-Origin", origin);
		    }

		    var usersController = DependencyResolver.Current.GetService<UsersController>();
			usersController.SetLocale(null, HttpContext.Request.UserLanguages);

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

					var stringBuilder = new StringBuilder();
					if (result != null)
					{
						var jsonService = DependencyResolver.Current.GetService<IJsonService>();
						var outputWriter = jsonService.CreateWriter(new StringWriter(stringBuilder), true);
						jsonService.CreateSerializer().Serialize(outputWriter, result);
					}

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
						if (e is AccessForbiddenException || e is NoSuchProjectMemberException || e is NoSuchUserException)
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

						return Json(new
						{
							message = message.ToString(),
							invalidProject = e is NoSuchProjectException,
							accessDenied =  e is NoSuchUserException || e is NoSuchProjectMemberException
						});
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages.Scope;
using AGO.Core;
using AGO.Core.Execution;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Reporting.Common;
using Common.Logging;

namespace AGO.Reporting.Service.Controllers
{
	public class ReportingApiController: Controller
	{
		public ActionResult Dispatch()
		{
			using (ScopeStorage.CreateTransientScope(new Dictionary<object, object>()))
			{
				var logged = false;
				try
				{
					var service = RouteData.Values["service"] as IReportingService;
					if (service == null)
						throw new Exception("service is null");
					var method = service.GetType().GetMethod(RouteData.Values["method"] as string);
					if (method == null)
						throw new Exception("method is null");

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

						return Json(new {message = message.ToString()});
					}
					catch (Exception ex)
					{
						LogException(ex); //fatal, but we need cause of exception in log
						throw;
					}
				}
			}

		}

		public ActionResult Error()
		{
			return new HttpNotFoundResult();
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
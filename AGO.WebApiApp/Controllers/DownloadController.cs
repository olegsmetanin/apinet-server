using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AGO.Core;
using AGO.Core.Controllers;
using AGO.Core.Localization;
using Common.Logging;

namespace AGO.WebApiApp.Controllers
{
	public class DownloadController: Controller
	{
		public ActionResult DownloadReportTemplate(Guid id)
		{
			return Download(Downloader.REPORT_TEMPLATE_TYPE, id);
		}

		public ActionResult DownloadReport(Guid id)
		{
			return Download(Downloader.REPORT_TYPE, id);
		}

		public ActionResult DownloadFile(Guid id)
		{
			return Download(Downloader.REPORT_TYPE, id);
		}

		private ActionResult Download(string type, Guid id)
		{
			try
			{
				var authController = DependencyResolver.Current.GetService<AuthController>();
				if (authController == null)
					throw new NotAuthenticatedException();

				if (!authController.IsAuthenticated())
					throw new NotAuthenticatedException();

				var downloader = DependencyResolver.Current.GetService<Downloader>();
				downloader.ServeDownloadWebRequest(Request, type, id); 
				return null;
			}
			catch (Exception e)
			{
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

					return Json(new { message = message.ToString() }, JsonRequestBehavior.AllowGet);
				}
				catch (Exception ex)
				{
					LogException(ex);//fatal, but we need cause of exception in log
					throw;
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
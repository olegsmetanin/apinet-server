using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AGO.Core;
using AGO.Core.Localization;
using Common.Logging;

namespace AGO.WebApiApp.Controllers
{
	/// <summary>
	/// Base class for web-related controllers. Contains only error handling logic, but must 
	/// prepare sync context in future (context = current user + locale + current session and similar stuff, may be own 
	/// implementation for SynchronizationContext (or use asp.net version) for async methods support).
	/// </summary>
	public abstract class BaseMvcController: Controller
	{
		protected Task<ActionResult> DoSafeAsync(Func<Task<ActionResult>> protectedAction)
		{
			try
			{
				return protectedAction();
			}
			catch (Exception ex)
			{
				return Task.FromResult<ActionResult>(HandleException(ex));
			}
		}

		protected ActionResult DoSafe(Func<ActionResult> protectedAction)
		{
			try
			{
				return protectedAction();
			}
			catch (Exception ex)
			{
				return HandleException(ex);
			}
		}

		private JsonResult HandleException(Exception e)
		{
			try
			{
				LogException(e);

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

				return Json(new {message = message.ToString()}, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				LogException(ex); //fatal, but we need cause of exception in log
				throw;
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
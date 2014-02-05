using System;
using System.Web.Mvc;
using AGO.Core;
using AGO.Core.Controllers;
using AGO.Core.Controllers.Security;

namespace AGO.WebApiApp.Controllers
{
	public class DownloadController: BaseMvcController
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
			return DoSafe(() =>
			{
				var authController = DependencyResolver.Current.GetService<AuthController>();
				if (authController == null)
					throw new NotAuthenticatedException();

				if (!authController.IsAuthenticated())
					throw new NotAuthenticatedException();

				var downloader = DependencyResolver.Current.GetService<Downloader>();
				downloader.ServeDownloadWebRequest(Request, type, id);
				return null;
			});
		}
	}
}
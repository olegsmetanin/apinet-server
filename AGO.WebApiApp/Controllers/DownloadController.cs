using System;
using System.Web.Mvc;
using AGO.Core;
using AGO.Core.Controllers;
using AGO.Core.Controllers.Security;

namespace AGO.WebApiApp.Controllers
{
	public class DownloadController: BaseMvcController
	{
		public ActionResult DownloadReportTemplate(string project, Guid id)
		{
			return Download(Downloader.REPORT_TEMPLATE_TYPE, project, id);
		}

		public ActionResult DownloadReport(string project, Guid id)
		{
			return Download(Downloader.REPORT_TYPE, project, id);
		}

		public ActionResult DownloadFile(string project, Guid id)
		{
			return Download(Downloader.FILE_TYPE, project, id);
		}

		private ActionResult Download(string type, string project, Guid id)
		{
			return DoSafe(() =>
			{
				var authController = DependencyResolver.Current.GetService<AuthController>();
				if (authController == null)
					throw new NotAuthenticatedException();

				if (!authController.IsAuthenticated())
					throw new NotAuthenticatedException();

				var downloader = DependencyResolver.Current.GetService<Downloader>();
				downloader.ServeDownloadWebRequest(Request, type, project, id);
				return null;
			});
		}
	}
}
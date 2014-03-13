using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using AGO.Core.Controllers.Security;
using AGO.Core.Localization;
using AGO.Core.Model;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Reporting;
using AGO.Core.Model.Security;
using AGO.Core.Notification;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;
using SimpleInjector;

namespace AGO.Core.Controllers
{

	/// <summary>
	/// Вспомогательный класс для получения доступа к сохраненным в системе файлам.
	/// В т.ч. реализует логику отдачи файла по http (основной сценарий использования, 99%)
	/// </summary>
	public class Downloader
	{
		public const string REPORT_TEMPLATE_TYPE = "report-template";
		public const string REPORT_TYPE = "report";
		public const string FILE_TYPE = "file";
		private readonly Container diContainer;

		public Downloader(Container diContainer)
		{
			if (diContainer == null)
				throw new ArgumentNullException("diContainer");

			this.diContainer = diContainer;
		}

		public void ServeDownloadWebRequest(HttpRequestBase request, string type, string project, Guid id)
		{
			
			var sp = diContainer.GetInstance<ISessionProvider>();
			switch (type)
			{
				case REPORT_TEMPLATE_TYPE:
					//TODO security for downloading
					var template = sp.CurrentSession.Get<ReportTemplateModel>(id);
					if (template == null)
					{
						NotFound(request.RequestContext.HttpContext.Response, "Report template with given id not exists");
					}
					else
					{
						var wrapper = new TemplateWrapper(template);
						Send(request, wrapper);
					}
					break;
				case REPORT_TYPE:
					//TODO security for downloading
					var report = sp.CurrentSession.Get<ReportTaskModel>(id);
					if (report == null)
					{
						NotFound(request.RequestContext.HttpContext.Response, "Report with given id not exists");
					}
					else if (report.ResultContent == null)
					{
						NotFound(request.RequestContext.HttpContext.Response, "Report without result");
					}
					else
					{
						var wrapper = new ReportWrapper(report);
						Send(request, wrapper);
						if (report.ResultUnread)
						{
							report.ResultUnread = false;
							diContainer.GetInstance<ICrudDao>().Store(report);

							var lc = diContainer.GetInstance<ILocalizationService>();
							var user = diContainer.GetInstance<AuthController>().CurrentUser();
							var p = sp.CurrentSession.QueryOver<ProjectModel>().Where(m => m.ProjectCode == report.Project).SingleOrDefault();
							//User must be logged in to download report, so, we don't check user to null
							var dto = ReportTaskDTO.FromTask(report, lc, p != null ? p.Name : null, user.SystemRole != SystemRole.Administrator);
							diContainer.GetInstance<INotificationService>().EmitReportChanged(ReportEvents.DOWNLOADED, user.Email, dto);

							sp.FlushCurrentSession();
						}
					}
					break;
				case FILE_TYPE:
					//security implemented in storages
					var storages = diContainer.GetAllInstances<IFileResourceStorage>();
					IFileResource file = null;
					foreach (var s in storages)
					{
						file = s.FindFile(project, id);
						if (file != null) break;
					}
					if (file == null)
					{
						NotFound(request.RequestContext.HttpContext.Response, "No file with this id in project");
					}
					else
					{
						Send(request, file);
					}
					break;
				default:
					NotFound(request.RequestContext.HttpContext.Response, "Unknown resource type: " + type);
					break;
			}
		}

		private void Send(HttpRequestBase request, IFileResource file, bool inline = false)
		{
			var context = request.RequestContext.HttpContext;
			var response = context.Response;
			try
			{
				if (UseCachedVersionIfNotModified(file.LastChange, context))
					return;
				
				response.ContentType = file.ContentType ?? MimeAssistant.GetMimeType(file.FileName);
				var contentDisposition = MakeContentDispositionHeader(file.FileName, request, inline);
				if (!contentDisposition.IsNullOrWhiteSpace())
					response.AppendHeader("Content-Disposition", contentDisposition);
				response.AppendHeader("Accept-Ranges", "bytes");

				int start;
				int length;
				var statusCode = 200;
				var partial = GetRanges(request, file.Content, out start, out length);
				if (partial)
				{
					statusCode = 206;
					var rangeValue = string.Format("bytes {0}-", start);
					if (length > 0)
						rangeValue += (length - 1);
					if (file.Content.Length > 0)
						rangeValue += string.Format("/{0}", file.Content.Length);
					response.AppendHeader("Content-Range", rangeValue);
				}
				if (length > 0)
					response.AppendHeader("Content-Length", string.Format("{0}", length));

				file.Content.WriteRange(response.OutputStream, start, length);

				response.StatusCode = statusCode;
			}
			finally
			{
				file.Content.Close();
			}
		}

		private void NotFound(HttpResponseBase response, string reason)
		{
			response.StatusCode = 404;
			response.StatusDescription = reason;
		}

		private string MakeContentDispositionHeader(string fileName, HttpRequestBase request, bool inline)
		{
			if (!fileName.IsNullOrEmpty())
			{
				var builder = new StringBuilder();
				var invalidChars = Path.GetInvalidPathChars();
				foreach (var c in fileName.Where(c => !invalidChars.Any(ic => c == ic)))
					builder.Append(c);
				fileName = builder.ToString();

				var disposition = inline ? "inline" : "attachment";

				string dispositionHeader;
				if ("IE".Equals(request.Browser.Browser) && 
						("7.0".Equals(request.Browser.Version) || "8.0".Equals(request.Browser.Version)))
					dispositionHeader = String.Format("{0}; filename={1}", disposition, Uri.EscapeDataString(fileName.Replace(' ', '_')));
				else if ("Safari".Equals(request.Browser.Browser))
					dispositionHeader = String.Format("{0}; filename={1}", disposition, fileName);
				else
					dispositionHeader = String.Format("{0}; filename*=UTF-8''{1}", disposition, Uri.EscapeDataString(fileName));
				
				return dispositionHeader;
			}
			return null;
		}

		private bool UseCachedVersionIfNotModified(
			DateTime modifyTime,
			HttpContextBase httpContext,
			TimeSpan timeToLive = default(TimeSpan))
		{
			var modifyTimeUtc = modifyTime.ToUniversalTime();
			httpContext.Response.Cache.SetCacheability(HttpCacheability.Private);
			httpContext.Response.Cache.SetLastModified(modifyTimeUtc);

			if (timeToLive != default(TimeSpan))
				httpContext.Response.Cache.SetExpires(DateTime.UtcNow.Add(timeToLive));

			var header = httpContext.Request.Headers.Get("If-Modified-Since");
			if (header.IsNullOrEmpty())
				return false;

			var sinceTime = header.ConvertSafe<DateTime?>();
			if (sinceTime == null)
				return false;

			var elapsedFromChange = sinceTime.Value.ToUniversalTime() - modifyTimeUtc;
			if (Math.Ceiling(elapsedFromChange.TotalSeconds) < 0)
				return false;

			httpContext.Response.StatusCode = 304;
			return true;
		}

		private bool GetRanges(HttpRequestBase request, Stream stream, out int start, out int length)
		{
			var result = false;
			int? calculatedStart = null;
			int? calculatedEnd = null;

			var header = request.Headers.Get("Range");
			if (!header.IsNullOrEmpty())
			{
				var regex = new Regex(@"(\d*)\s*-\s*(\d*)");
				var match = regex.Match(header);
				if (match.Success)
				{
					try
					{
						calculatedStart = Convert.ToInt32(match.Groups[1].Value);
					}
					catch
					{
						calculatedStart = null;
					}
					try
					{
						calculatedEnd = Convert.ToInt32(match.Groups[2].Value);
					}
					catch
					{
						calculatedEnd = null;
					}
					if (calculatedStart != null || calculatedEnd != null)
						result = true;
				}
			}

			if (calculatedStart == null || calculatedStart.Value < 0)
				calculatedStart = 0;
			if (calculatedStart.Value > stream.Length)
				calculatedStart = (int)stream.Length;
			if (calculatedEnd == null || calculatedEnd.Value >= stream.Length)
				calculatedEnd = (int)stream.Length - 1;
			if (calculatedEnd.Value < calculatedStart.Value)
				calculatedEnd = calculatedStart.Value - 1;

			start = calculatedStart.Value;
			length = calculatedEnd.Value - calculatedStart.Value + 1;
			return result;
		}

		private class TemplateWrapper: IFileResource
		{
			private readonly IReportTemplate template;

			public TemplateWrapper(IReportTemplate template)
			{
				this.template = template;
			}

			public string FileName
			{
				get { return template.Name; }
			}

			public string ContentType
			{
				get { return MimeAssistant.GetMimeType(FileName); }
			}

			public DateTime LastChange
			{
				get { return template.LastChange; }
			}

			public Stream Content
			{
				get { return template.Content != null ? new MemoryStream(template.Content) : null; }
			}
		}

		private class ReportWrapper: IFileResource
		{
			private readonly ReportTaskModel task;

			public ReportWrapper(ReportTaskModel task)
			{
				this.task = task;
			}

			public string FileName
			{
				get { return task.ResultName; }
			}

			public string ContentType
			{
				get { return task.ResultContentType ?? MimeAssistant.GetMimeType(task.ResultName); }
			}

			public DateTime LastChange
			{
				get { return task.LastChangeTime ?? task.CreationTime.GetValueOrDefault(); }
			}

			public Stream Content
			{
				get { return task.ResultContent != null ? new MemoryStream(task.ResultContent) : null; }
			}
		}
	}
}
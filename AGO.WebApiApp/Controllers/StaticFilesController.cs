using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using AGO.Core;
using AGO.Core.Controllers;
using AGO.WebApiApp.Application;

namespace AGO.WebApiApp.Controllers
{
	public class StaticFilesController : Controller
	{
		#region Constants

		public const string DevRoot = "~/ng-app/src/";

		public const string ProdRoot = "~/ng-app/release/";

		public const int DaysToLive = 30;

		#endregion

		#region Controller actions

		public ActionResult StaticFile(string path)
		{
			try
			{
				var originPath = WebApplication.DevMode == DevMode.Dev ? DevRoot : ProdRoot;
				var originDirInfo = new DirectoryInfo(Server.MapPath(originPath));
				if (!originDirInfo.Exists)
					throw new Exception("Origin folder not exists");

				var resultPath = Path.Combine(originDirInfo.FullName, path);
				var resultFileInfo = new FileInfo(resultPath);
				if (!resultFileInfo.Exists)
					throw new Exception("File not exists");
				if (!resultFileInfo.FullName.StartsWith(originDirInfo.FullName))
					throw new Exception("File is not in origin subtree");

				ServeFile(resultFileInfo);

				return null;
			}
			catch (Exception e)
			{
				if (e is HttpException)
					throw;
				throw new HttpException(404, e.Message);
			}
		}
 
		#endregion

		#region Helper methods

		protected void ServeFile(FileInfo fileInfo)
		{
			var stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);

			try
			{
				if (UseCachedVersionIfNotModified(fileInfo.LastWriteTime, HttpContext, TimeSpan.FromDays(DaysToLive)))
					return;

				/*var fileName = fileInfo.Name;
				if (!fileName.IsNullOrEmpty())
				{
					var builder = new StringBuilder();
					var invalidChars = Path.GetInvalidPathChars();
					foreach (var c in fileName.Where(c => !invalidChars.Any(ic => c == ic)))
						builder.Append(c);
					fileName = builder.ToString();

					const string disposition = "inline";

					string dispositionHeader;
					if ("IE".Equals(HttpContext.Request.Browser.Browser) && 
							("7.0".Equals(HttpContext.Request.Browser.Version) || "8.0".Equals(HttpContext.Request.Browser.Version)))
						dispositionHeader = String.Format("{0}; filename={1}", disposition, Uri.EscapeDataString(fileName.Replace(' ', '_')));
					else if ("Safari".Equals(HttpContext.Request.Browser.Browser))
						dispositionHeader = String.Format("{0}; filename={1}", disposition, fileName);
					else
						dispositionHeader = String.Format("{0}; filename*=UTF-8''{1}", disposition, Uri.EscapeDataString(fileName));
					HttpContext.Response.AddHeader("Content-Disposition", dispositionHeader);
				}*/

				HttpContext.Response.ContentType = MimeAssistant.GetMimeType(fileInfo.Name);

				HttpContext.Response.AppendHeader("Accept-Ranges", "bytes");

				int start;
				int length;
				var statusCode = 200;
				var partial = GetRanges(stream, out start, out length);
				if (partial)
				{
					statusCode = 206;
					var rangeValue = string.Format("bytes {0}-", start);
					if (length > 0)
						rangeValue += (length - 1);
					if (stream.Length > 0)
						rangeValue += string.Format("/{0}", stream.Length);
					HttpContext.Response.AppendHeader("Content-Range", rangeValue);
				}
				if (length > 0)
					HttpContext.Response.AppendHeader("Content-Length", string.Format("{0}", length));

				stream.WriteRange(HttpContext.Response.OutputStream, start, length);

				HttpContext.Response.StatusCode = statusCode;
			}
			finally
			{
				stream.Close();
			}
		}

		protected bool UseCachedVersionIfNotModified(
			DateTime modifyTime,
			HttpContextBase httpContext,
			TimeSpan timeToLive = default(TimeSpan))
		{
			if (WebApplication.DisableCaching)
			{
				httpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
				return false;
			}

			var modifyTimeUtc = modifyTime.ToUniversalTime();
			httpContext.Response.Cache.SetCacheability(HttpCacheability.Public);
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

		protected bool GetRanges(Stream stream, out int start, out int length)
		{
			var result = false;
			var request = HttpContext.Request;
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

		#endregion
	}
}
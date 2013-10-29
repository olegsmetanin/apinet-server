using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using AGO.Core;
using AGO.WebApiApp.App_Start;

namespace AGO.WebApiApp.Controllers
{
	public class StaticFilesController : Controller
	{
		#region Constants

		public const string DevRoot = "~/ng-app/";

		public const string ProdRoot = "~/ng-app/dist/";

		#endregion

		#region Nested classes

		protected static class MimeAssistant
		{
			private static readonly Dictionary<string, string> MimeTypesDictionary
				= new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
			{
				{"ai", "application/postscript"},
				{"aif", "audio/x-aiff"},
				{"aifc", "audio/x-aiff"},
				{"aiff", "audio/x-aiff"},
				{"asc", "text/plain"},
				{"atom", "application/atom+xml"},
				{"au", "audio/basic"},
				{"avi", "video/x-msvideo"},
				{"bcpio", "application/x-bcpio"},
				{"bin", "application/octet-stream"},
				{"bmp", "image/bmp"},
				{"cdf", "application/x-netcdf"},
				{"cgm", "image/cgm"},
				{"class", "application/octet-stream"},
				{"cpio", "application/x-cpio"},
				{"cpt", "application/mac-compactpro"},
				{"csh", "application/x-csh"},
				{"css", "text/css"},
				{"dcr", "application/x-director"},
				{"dif", "video/x-dv"},
				{"dir", "application/x-director"},
				{"djv", "image/vnd.djvu"},
				{"djvu", "image/vnd.djvu"},
				{"dll", "application/octet-stream"},
				{"dmg", "application/octet-stream"},
				{"dms", "application/octet-stream"},
				{"doc", "application/msword"},
				{"docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
				{"dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
				{"docm","application/vnd.ms-word.document.macroEnabled.12"},
				{"dotm","application/vnd.ms-word.template.macroEnabled.12"},
				{"dtd", "application/xml-dtd"},
				{"dv", "video/x-dv"},
				{"dvi", "application/x-dvi"},
				{"dxr", "application/x-director"},
				{"eps", "application/postscript"},
				{"etx", "text/x-setext"},
				{"exe", "application/octet-stream"},
				{"ez", "application/andrew-inset"},
				{"gif", "image/gif"},
				{"gram", "application/srgs"},
				{"grxml", "application/srgs+xml"},
				{"gtar", "application/x-gtar"},
				{"hdf", "application/x-hdf"},
				{"hqx", "application/mac-binhex40"},
				{"htm", "text/html"},
				{"html", "text/html"},
				{"ice", "x-conference/x-cooltalk"},
				{"ico", "image/x-icon"},
				{"ics", "text/calendar"},
				{"ief", "image/ief"},
				{"ifb", "text/calendar"},
				{"iges", "model/iges"},
				{"igs", "model/iges"},
				{"jnlp", "application/x-java-jnlp-file"},
				{"jp2", "image/jp2"},
				{"jpe", "image/jpeg"},
				{"jpeg", "image/jpeg"},
				{"jpg", "image/jpeg"},
				{"js", "application/x-javascript"},
				{"kar", "audio/midi"},
				{"latex", "application/x-latex"},
				{"lha", "application/octet-stream"},
				{"lzh", "application/octet-stream"},
				{"m3u", "audio/x-mpegurl"},
				{"m4a", "audio/mp4a-latm"},
				{"m4b", "audio/mp4a-latm"},
				{"m4p", "audio/mp4a-latm"},
				{"m4u", "video/vnd.mpegurl"},
				{"m4v", "video/x-m4v"},
				{"mac", "image/x-macpaint"},
				{"man", "application/x-troff-man"},
				{"mathml", "application/mathml+xml"},
				{"me", "application/x-troff-me"},
				{"mesh", "model/mesh"},
				{"mid", "audio/midi"},
				{"midi", "audio/midi"},
				{"mif", "application/vnd.mif"},
				{"mov", "video/quicktime"},
				{"movie", "video/x-sgi-movie"},
				{"mp2", "audio/mpeg"},
				{"mp3", "audio/mpeg"},
				{"mp4", "video/mp4"},
				{"mpe", "video/mpeg"},
				{"mpeg", "video/mpeg"},
				{"mpg", "video/mpeg"},
				{"mpga", "audio/mpeg"},
				{"ms", "application/x-troff-ms"},
				{"msh", "model/mesh"},
				{"mxu", "video/vnd.mpegurl"},
				{"nc", "application/x-netcdf"},
				{"oda", "application/oda"},
				{"ogg", "application/ogg"},
				{"pbm", "image/x-portable-bitmap"},
				{"pct", "image/pict"},
				{"pdb", "chemical/x-pdb"},
				{"pdf", "application/pdf"},
				{"pgm", "image/x-portable-graymap"},
				{"pgn", "application/x-chess-pgn"},
				{"pic", "image/pict"},
				{"pict", "image/pict"},
				{"png", "image/png"}, 
				{"pnm", "image/x-portable-anymap"},
				{"pnt", "image/x-macpaint"},
				{"pntg", "image/x-macpaint"},
				{"ppm", "image/x-portable-pixmap"},
				{"ppt", "application/vnd.ms-powerpoint"},
				{"pptx","application/vnd.openxmlformats-officedocument.presentationml.presentation"},
				{"potx","application/vnd.openxmlformats-officedocument.presentationml.template"},
				{"ppsx","application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
				{"ppam","application/vnd.ms-powerpoint.addin.macroEnabled.12"},
				{"pptm","application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
				{"potm","application/vnd.ms-powerpoint.template.macroEnabled.12"},
				{"ppsm","application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
				{"ps", "application/postscript"},
				{"qt", "video/quicktime"},
				{"qti", "image/x-quicktime"},
				{"qtif", "image/x-quicktime"},
				{"ra", "audio/x-pn-realaudio"},
				{"ram", "audio/x-pn-realaudio"},
				{"ras", "image/x-cmu-raster"},
				{"rdf", "application/rdf+xml"},
				{"rgb", "image/x-rgb"},
				{"rm", "application/vnd.rn-realmedia"},
				{"roff", "application/x-troff"},
				{"rtf", "text/rtf"},
				{"rtx", "text/richtext"},
				{"sgm", "text/sgml"},
				{"sgml", "text/sgml"},
				{"sh", "application/x-sh"},
				{"shar", "application/x-shar"},
				{"silo", "model/mesh"},
				{"sit", "application/x-stuffit"},
				{"skd", "application/x-koan"},
				{"skm", "application/x-koan"},
				{"skp", "application/x-koan"},
				{"skt", "application/x-koan"},
				{"smi", "application/smil"},
				{"smil", "application/smil"},
				{"snd", "audio/basic"},
				{"so", "application/octet-stream"},
				{"spl", "application/x-futuresplash"},
				{"src", "application/x-wais-source"},
				{"sv4cpio", "application/x-sv4cpio"},
				{"sv4crc", "application/x-sv4crc"},
				{"svg", "image/svg+xml"},
				{"swf", "application/x-shockwave-flash"},
				{"t", "application/x-troff"},
				{"tar", "application/x-tar"},
				{"tcl", "application/x-tcl"},
				{"tex", "application/x-tex"},
				{"texi", "application/x-texinfo"},
				{"texinfo", "application/x-texinfo"},
				{"tif", "image/tiff"},
				{"tiff", "image/tiff"},
				{"tr", "application/x-troff"},
				{"tsv", "text/tab-separated-values"},
				{"txt", "text/plain"},
				{"ustar", "application/x-ustar"},
				{"vcd", "application/x-cdlink"},
				{"vrml", "model/vrml"},
				{"vxml", "application/voicexml+xml"},
				{"wav", "audio/x-wav"},
				{"wbmp", "image/vnd.wap.wbmp"},
				{"wbmxl", "application/vnd.wap.wbxml"},
				{"wml", "text/vnd.wap.wml"},
				{"wmlc", "application/vnd.wap.wmlc"},
				{"wmls", "text/vnd.wap.wmlscript"},
				{"wmlsc", "application/vnd.wap.wmlscriptc"},
				{"wrl", "model/vrml"},
				{"xbm", "image/x-xbitmap"},
				{"xht", "application/xhtml+xml"},
				{"xhtml", "application/xhtml+xml"},
				{"xls", "application/vnd.ms-excel"},                        
				{"xml", "application/xml"},
				{"xpm", "image/x-xpixmap"},
				{"xsl", "application/xml"},
				{"xlsx","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
				{"xltx","application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
				{"xlsm","application/vnd.ms-excel.sheet.macroEnabled.12"},
				{"xltm","application/vnd.ms-excel.template.macroEnabled.12"},
				{"xlam","application/vnd.ms-excel.addin.macroEnabled.12"},
				{"xlsb","application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
				{"xslt", "application/xslt+xml"},
				{"xul", "application/vnd.mozilla.xul+xml"},
				{"xwd", "image/x-xwindowdump"},
				{"xyz", "chemical/x-xyz"},
				{"zip", "application/zip"}
			};

			public static string GetMimeType(string fileName)
			{
				var result = "application/octet-stream";
				if (!fileName.IsNullOrWhiteSpace())
				{
					var extension = Path.GetExtension(fileName).TrimSafe().RemovePrefix(".");
					if (MimeTypesDictionary.ContainsKey(extension))
						result = MimeTypesDictionary[extension];
				}
				return result;
			}
		}

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
			//MimeAssistant.GetMimeType(resultFileInfo.Name)

			try
			{
				if (UseCachedVersionIfNotModified(fileInfo.LastWriteTime, HttpContext))
					return;

				var fileName = fileInfo.Name;
				if (!fileName.IsNullOrEmpty())
				{
					var builder = new StringBuilder();
					var invalidChars = Path.GetInvalidPathChars();
					foreach (var c in fileName.Where(c => !invalidChars.Any(ic => c == ic)))
						builder.Append(c);
					fileName = builder.ToString();

					const string disposition = "attachment";/* : "inline";*/

					string dispositionHeader;
					if ("IE".Equals(HttpContext.Request.Browser.Browser) && 
							("7.0".Equals(HttpContext.Request.Browser.Version) || "8.0".Equals(HttpContext.Request.Browser.Version)))
						dispositionHeader = String.Format("{0}; filename={1}", disposition, Uri.EscapeDataString(fileName.Replace(' ', '_')));
					else if ("Safari".Equals(HttpContext.Request.Browser.Browser))
						dispositionHeader = String.Format("{0}; filename={1}", disposition, fileName);
					else
						dispositionHeader = String.Format("{0}; filename*=UTF-8''{1}", disposition, Uri.EscapeDataString(fileName));
					HttpContext.Response.AddHeader("Content-Disposition", dispositionHeader);
				}

				HttpContext.Response.ContentType = MimeAssistant.GetMimeType(fileName);

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
				httpContext.Response.Cache.SetExpires(modifyTime.Add(timeToLive));

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
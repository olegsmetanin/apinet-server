using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace AGO.Core.Controllers
{
	public class Uploader
	{
		private const string RANGE_HEADER = "Content-Range";
		private const string UPLOADID_FORM_FIELD = "uploadid";
		private readonly Regex rangeParser = new Regex(@"^bytes\s(\d+)\-(\d+)\/(\d+)$");
		private readonly string uploadDir;

		public Uploader(string tempDir)
		{
			uploadDir = !string.IsNullOrWhiteSpace(tempDir) ? tempDir : Path.GetTempPath();
		}

		public void HandleRequest(HttpRequestBase request, HttpPostedFileBase file, Action<string, byte[]> complete)
		{
			if (request == null)
				throw new ArgumentNullException("request");
			var fileName = Path.GetFileName(file.FileName);
			var bytes = request.Headers[RANGE_HEADER];
			var uploadid = request.Form[UPLOADID_FORM_FIELD];
			if (bytes.IsNullOrWhiteSpace() || !rangeParser.IsMatch(bytes))
			{
				//as single file
				var buffer = new byte[file.InputStream.Length];
				file.InputStream.Read(buffer, 0, buffer.Length);
				complete(fileName, buffer);
			}
			else
			{
				//temp folder for storing chunks
				var folder = Path.Combine(uploadDir, uploadid);
				if (!Directory.Exists(folder))
					Directory.CreateDirectory(folder);
				
				var matches = rangeParser.Match(bytes);
				var from = int.Parse(matches.Groups[1].Value);
				var to = int.Parse(matches.Groups[2].Value);
				var size = int.Parse(matches.Groups[3].Value);

				var path = Path.Combine(folder, from.ToString("D10"));
				file.SaveAs(path);

				if ((size - to) <= 1)
				{
					//this is last chunck
					var files = Directory.GetFiles(folder);
					Array.Sort(files);
					try
					{
						var buffer = new byte[size];
						var offset = 0;
						foreach (var fn in files)
						{
							using(var fs = File.OpenRead(fn))
							{
								offset = fs.Read(buffer, offset, (int)fs.Length);
								fs.Close();
							}
						}
						complete(fileName, buffer);
					}
					finally
					{
						foreach (var fn in files)
						{
							File.Delete(fn);
						}
						Directory.Delete(folder);
					}
				}
			}
		}
	}
}
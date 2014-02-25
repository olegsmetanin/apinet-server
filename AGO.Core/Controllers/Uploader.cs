using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace AGO.Core.Controllers
{
	/// <summary>
	/// Вспомогательный класс для обработки запросов на закачку файлов
	/// </summary>
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
			HandleRequest(request, file, (fileName, content) =>
			{
				var buffer = new byte[content.Length];
				content.Read(buffer, 0, buffer.Length);
				complete(fileName, buffer);
			});
		}

		public void HandleRequest(HttpRequestBase request, HttpPostedFileBase file, Action<string, Stream> complete)
		{
			if (request == null)
				throw new ArgumentNullException("request");
			var fileName = Path.GetFileName(file.FileName.Normalize()); //bug with ios unicode support (й as two separate symbols);
			var bytes = request.Headers[RANGE_HEADER];
			var uploadid = request.Form[UPLOADID_FORM_FIELD];
			if (uploadid.IsNullOrWhiteSpace())
			{
				throw new ArgumentException("UploadId parameter not found in form data", "request");
			}
			if (bytes.IsNullOrWhiteSpace() || !rangeParser.IsMatch(bytes))
			{
				//as single file
				complete(fileName, file.InputStream);
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
					try
					{
						var files = Directory.GetFiles(folder);
						Array.Sort(files);
						//combine chunks to single file in order
						var combinedFileName = Path.Combine(folder, uploadid);
						using (var trg = File.OpenWrite(combinedFileName))
						{
							foreach (var fn in files)
							{
								using (var fs = File.OpenRead(fn))
								{
									fs.CopyTo(trg);
									fs.Close();
								}
							}
							trg.Close();
						}
						//call callback with combined file result
						using (var cmb = File.OpenRead(combinedFileName))
						{
							complete(fileName, cmb);
							cmb.Close();
						}
					}
					finally
					{
						Directory.Delete(folder, true);
					}
				}
			}
		}
	}
}
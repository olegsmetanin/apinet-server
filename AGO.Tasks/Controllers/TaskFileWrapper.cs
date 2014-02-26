using System;
using System.IO;
using AGO.Core;
using AGO.Core.Controllers;
using AGO.Core.Model;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Controllers
{
	internal sealed class TaskFileWrapper: IFileResource
	{
		internal TaskFileWrapper(string storageRoot, TaskFileModel file)
		{
			if (storageRoot.IsNullOrWhiteSpace())
				throw new ArgumentNullException("storageRoot");
			if (file == null)
				throw new ArgumentNullException("file");
			if (!file.Uploaded || file.Path.IsNullOrWhiteSpace())
				throw new ArgumentException("Inconsistent file data", "file");

			FileName = file.Name;
			ContentType = !file.ContentType.IsNullOrWhiteSpace() 
				? file.ContentType : MimeAssistant.GetMimeType(file.Name);
			LastChange = file.LastChangeTime.GetValueOrDefault();
			Content = File.OpenRead(Path.Combine(storageRoot, file.Path));
		}

		public string FileName { get; private set; }
		public string ContentType { get; private set; }
		public DateTime LastChange { get; private set; }
		public Stream Content { get; private set; }
	}
}
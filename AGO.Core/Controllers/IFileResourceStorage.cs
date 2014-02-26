using System;
using AGO.Core.Model;

namespace AGO.Core.Controllers
{
	/// <summary>
	/// Needs to implement and register in modules for <see cref="Downloader"/> can access concrete files
	/// by project and id
	/// </summary>
	public interface IFileResourceStorage
	{
		/// <summary>
		/// Try to find file and return as generic file resource (for downloading)
		/// </summary>
		IFileResource FindFile(string project, Guid fileId);
	}
}
using System;
using System.IO;

namespace AGO.Core.Model
{
	/// <summary>
	/// For models, that represent stored file
	/// </summary>
	public interface IFileResource
	{
		string FileName { get; }

		string ContentType { get; }

		DateTime LastChange { get; }

		Stream Content { get; }
	}
}
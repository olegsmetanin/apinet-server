using System.Collections.Generic;

namespace AGO.Core.Model.Files
{
	/// <summary>
	/// Interface for entities, that owns a file collection (task, document and so on).
	/// Implemented in modules.
	/// </summary>
	public interface IFileOwner<in TOwner, TFile>: IProjectBoundModel
		where TOwner: IFileOwner<TOwner, TFile> 
		where TFile: IFile<TOwner, TFile>
	{
		/// <summary>
		/// Attached files
		/// </summary>
		ISet<TFile> Files { get; }
	}
}
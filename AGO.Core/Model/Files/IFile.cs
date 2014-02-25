namespace AGO.Core.Model.Files
{
	/// <summary>
	/// Interface for entities, that represent file, attached to registry record (task, document and so on).
	/// Implemented in modules.
	/// </summary>
	public interface IFile<out TOwner, TFile> 
		where TOwner: IFileOwner<TOwner, TFile> 
		where TFile: IFile<TOwner, TFile>
	{
		TOwner Owner { get; }
	}
}
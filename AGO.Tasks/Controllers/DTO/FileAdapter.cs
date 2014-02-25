using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Controllers.DTO
{
	/// <summary>
	/// Adapter for task file dto
	/// </summary>
	public class FileAdapter: ModelAdapter<TaskFileModel, FileDTO>
	{
		public override FileDTO Fill(TaskFileModel model)
		{
			var dto = base.Fill(model);
			dto.Author = ToAuthor(model);
			dto.CreationTime = model.CreationTime;
			dto.Name = model.Name;
			dto.Size = model.Size;
			dto.Uploaded = model.Uploaded;
			return dto;
		}
	}
}
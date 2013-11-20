using AGO.Tasks.Model.Dictionary;

namespace AGO.Tasks.Controllers.DTO
{
	/// <summary>
	/// Адаптер моделей справочника типов задач
	/// </summary>
	public class TaskTypeAdapter: DictionaryModelAdapter<TaskTypeModel, TaskTypeDTO>
	{
	}

	/// <summary>
	/// Адаптер моделей справочника тегов задач
	/// </summary>
	public class TaskTagAdapter: DictionaryModelAdapter<TaskTagModel, TaskTagDTO>
	{
		public override TaskTagDTO Fill(TaskTagModel model)
		{
			var dto = base.Fill(model);
			dto.Name = model.FullName;
			return dto;
		}
	}
}
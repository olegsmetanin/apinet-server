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
	/// Адаптер моделей справочника пользовательских статусов задач
	/// </summary>
	public class CustomStatusAdapter: DictionaryModelAdapter<CustomTaskStatusModel, CustomStatusDTO>
	{
		public override CustomStatusDTO Fill(CustomTaskStatusModel model)
		{
			var dto = base.Fill(model);
			dto.ViewOrder = model.ViewOrder;
			return dto;
		}
	}
}
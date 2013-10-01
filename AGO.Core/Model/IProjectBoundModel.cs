namespace AGO.Core.Model
{
	/// <summary>
	/// Интерфейс моделей, работающих в контексте проекта (практически все модели)
	/// </summary>
	public interface IProjectBoundModel
	{
		string ProjectCode { get; set; }
	}
}
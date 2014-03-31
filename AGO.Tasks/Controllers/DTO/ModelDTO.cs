using System;

namespace AGO.Tasks.Controllers.DTO
{
	/// <summary>
	/// Базовый класс для DTO моделей
	/// </summary>
	public abstract class ModelDTO
	{
		public Guid Id { get; set; }

		public int? ModelVersion { get; set; }
	}
	
	public abstract class DictionaryDTO: ModelDTO
	{
		public string Name { get; set; }

		public string Author { get; set; }

		public DateTime? CreationTime { get; set; }
	}
}
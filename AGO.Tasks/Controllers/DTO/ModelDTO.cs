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

	public class PropChangeDTO : ModelDTO
	{
		public PropChangeDTO()
		{
		}

		public PropChangeDTO(Guid id, int? version, string prop, object value = null)
		{
			Id = id;
			ModelVersion = version;
			Prop = prop;
			Value = value;
		}

		public string Prop { get; set; }

		public object Value { get; set; }
	}
}
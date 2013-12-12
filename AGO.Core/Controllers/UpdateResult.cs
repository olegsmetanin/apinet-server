using Newtonsoft.Json;

namespace AGO.Core.Controllers
{
	/// <summary>
	/// Результат изменения атрибутов сущности (в процессе создания или апдейта).
	/// По сути Tuple с результатами валидации и обновленной моделью.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class UpdateResult<T> where T: class
	{
		[JsonProperty("validation")]
		public ValidationResult Validation { get; set; }

		[JsonProperty("model")]
		public T Model { get; set; }
	}
}
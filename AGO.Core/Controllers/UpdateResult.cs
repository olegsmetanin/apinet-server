using Newtonsoft.Json;

namespace AGO.Core.Controllers
{
	/// <summary>
	/// ��������� ��������� ��������� �������� (� �������� �������� ��� �������).
	/// �� ���� Tuple � ������������ ��������� � ����������� �������.
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
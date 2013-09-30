using AGO.Core.Model.Dictionary;

namespace AGO.Tasks.Controllers.DTO
{
	public class CustomParameterDTO: ModelDTO
	{
		public string TypeName { get; set; }

		public CustomPropertyValueType ValueType { get; set; }

		public string Value { get; set; }
	}
}
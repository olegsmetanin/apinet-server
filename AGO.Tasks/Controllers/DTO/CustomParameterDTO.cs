using System;
using AGO.Core.Model.Dictionary;
using Newtonsoft.Json;

namespace AGO.Tasks.Controllers.DTO
{
	public class CustomParameterTypeDTO
	{
		[JsonProperty("id")]
		public Guid Id { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }

		public CustomPropertyValueType ValueType { get; set; }
	}

	public class CustomParameterDTO: ModelDTO
	{
		public CustomParameterTypeDTO Type { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public object Value { get; set; }
	}
}
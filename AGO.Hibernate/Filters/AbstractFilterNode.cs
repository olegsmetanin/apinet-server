using Newtonsoft.Json;

namespace AGO.Hibernate.Filters
{
	internal abstract class AbstractFilterNode : IFilterNode
	{
		[JsonProperty(PropertyName = "path")]
		public string Path { get; set; }

		[JsonProperty(PropertyName = "not", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool Negative { get; set; }

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
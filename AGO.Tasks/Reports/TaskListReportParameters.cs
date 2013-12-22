using AGO.Core.Filters;
using AGO.Tasks.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AGO.Tasks.Reports
{
	public class TaskListReportParameters
	{
		[JsonProperty("project")]
		public string Project { get; set; }

		[JsonProperty("filter")]
		public JObject Filter { get; set; }

		[JsonProperty("sorters")]
		public SortInfo[] Sorters { get; set; }

		[JsonProperty("predefined")]
		public TaskPredefinedFilter Predefined { get; set; }
	}
}
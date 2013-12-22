using System.Globalization;
using AGO.Core.Filters;

namespace AGO.Tasks.Reports
{
	public class DetailedTaskListDataGenerator: SimpleTaskListDataGenerator
	{
		public DetailedTaskListDataGenerator(IFilteringService service, IFilteringDao fdao) 
			: base(service, fdao)
		{
		}

		protected override void MakeItemValues(System.Xml.XmlElement item, Model.Task.TaskModel task)
		{
			base.MakeItemValues(item, task);

			MakeValue(item, "content", task.Content);
			MakeValue(item, "dueDate",
			          task.DueDate.HasValue ? task.DueDate.Value.ToString(CultureInfo.CurrentUICulture) : string.Empty);
		}
	}
}
using System;
using System.Linq;
using System.Xml;
using AGO.Core.Filters;
using AGO.Reporting.Common;
using AGO.Tasks.Controllers;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Reports
{
	public class SimpleTaskListDataGenerator: BaseReportDataGenerator
	{
		private readonly IFilteringService fs;
		private readonly IFilteringDao dao;

		public SimpleTaskListDataGenerator(IFilteringService service, IFilteringDao fdao)
		{
			if (service == null)
				throw new ArgumentNullException("service");
			if (fdao == null)
				throw new ArgumentNullException("fdao");

			fs = service;
			dao = fdao;
		}

		protected override void FillReportData(object parameters)
		{
			var param = parameters as TaskListReportParameters;
			if (param == null)
				throw new ArgumentException("parameters is not " + typeof(TaskListReportParameters).Name, "parameters");

			var filter = fs.ParseFilterSetFromJson(param.Filter);

			var projectPredicate = fs.Filter<TaskModel>().Where(m => m.ProjectCode == param.Project);
			var predefinedPredicate = param.Predefined.ToFilter(fs.Filter<TaskModel>());
			filter.Add(projectPredicate);
			if (predefinedPredicate != null)
				filter.Add(predefinedPredicate);

			var count = dao.RowCount<TaskModel>(filter);
			var range = MakeRange("data", "Tasks");

			if (count <= 0) return;

			InitTicker(count);
			var page = 0;
			var tasks = dao.List<TaskModel>(filter, page, param.Sorters);
			do
			{
				foreach (var task in tasks)
				{
					//TODO testing, remove
					System.Threading.Thread.Sleep(4000);

					var item = MakeItem(range);
					MakeItemValues(item, task);

					Ticker.AddTick();
				}

				page++;
				tasks = dao.List<TaskModel>(filter, page, param.Sorters);
			} while (tasks.Count > 0);
		}

		protected virtual void MakeItemValues(XmlElement item, TaskModel task)
		{
			MakeValue(item, "num", task.SeqNumber);
			MakeValue(item, "type", task.TaskType != null ? task.TaskType.Name : string.Empty);
			MakeValue(item, "executors", string.Join(", ", task.Executors.Select(e => e.Executor.User.FullName)));
		}
	}
}
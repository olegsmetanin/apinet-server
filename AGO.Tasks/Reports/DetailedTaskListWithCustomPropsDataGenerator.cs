using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using AGO.Core.Filters;
using AGO.Core.Localization;
using AGO.Core.Model.Dictionary;
using AGO.Reporting.Common;
using AGO.Tasks.Controllers;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Reports
{
	public class DetailedTaskListWithCustomPropsDataGenerator: BaseReportDataGenerator
	{
		private const int MAX_PROPS = 50;

		private readonly IFilteringService fs;
		private readonly IFilteringDao dao;
		private readonly ILocalizationService las;

		public DetailedTaskListWithCustomPropsDataGenerator(IFilteringService service, IFilteringDao fdao, ILocalizationService las)
		{
			if (service == null)
				throw new ArgumentNullException("service");
			if (fdao == null)
				throw new ArgumentNullException("fdao");
			if (las == null)
				throw new ArgumentNullException("las");

			fs = service;
			dao = fdao;
			this.las = las;
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
			var phrange = MakeRange("paramHeaders", "Tasks");
			var phItem = MakeItem(phrange);

			if (count <= 0) return;

			InitTicker(count);
			var page = 0;
			var tasks = dao.List<TaskModel>(filter, new FilteringOptions{
				Page = page, PageSize = 200, Sorters = param.Sorters });//default page size (20) too small for report generation
			do
			{
				foreach (var task in tasks)
				{
					var item = MakeItem(range);

					MakeValue(item, "num", task.SeqNumber);
					MakeValue(item, "type", task.TaskType != null ? task.TaskType.Name : string.Empty);
					MakeValue(item, "status", las.MessageForType(typeof(TaskStatus), task.Status));
					MakeValue(item, "priority", las.MessageForType(typeof (TaskPriority), task.Priority));
					MakeValue(item, "content", task.Content);
					MakeValue(item, "dueDate", task.DueDate.HasValue 
						? task.DueDate.Value.ToString(CultureInfo.CurrentUICulture) : string.Empty);
					MakeValue(item, "executors", string.Join(", ", task.Executors.Select(e => e.Executor.FullName)));
					int? expiration = null;
					if (task.DueDate.HasValue && task.Status != TaskStatus.Closed)
					{
						var diff = DateTime.UtcNow.Subtract(task.DueDate.Value).TotalDays;
						if (diff >= 0)
							expiration = Convert.ToInt32(Math.Floor(Math.Abs(diff)));
					}
					MakeValue(item, "expiration", expiration.ToString());//if null will be empty string, ok
					MakeValue(item, "tags", string.Join(", ", task.Tags.Select(l => l.Tag.FullName)));
					MakeValue(item, "statusHistory", string.Join("; ", task.StatusHistory
						.OrderBy(h => h.Start)
						.Select(h => string.Concat(
							las.MessageForType(typeof (TaskStatus), h.Status),
							" - ",
							(h.Creator != null ? h.Creator.FIO : "<no author>"),
							" - ",
							h.Start.ToString(CultureInfo.CurrentUICulture)))));

					MakeParamsPart(phItem, item, task.CustomProperties);
					
					Ticker.AddTick();
				}

				page++;
				tasks = dao.List<TaskModel>(filter, page, param.Sorters);
			} while (tasks.Count > 0);
		}

		private readonly Dictionary<CustomPropertyTypeModel, int> propsToColsMap = 
			new Dictionary<CustomPropertyTypeModel, int>(MAX_PROPS);
		
		private void MakeParamsPart(XmlElement headerItem, XmlElement rowItem, IEnumerable<CustomPropertyInstanceModel> props)
		{
			foreach (var p in props)
			{
				int propIndex;
				if (!propsToColsMap.TryGetValue(p.PropertyType, out propIndex))
				{
					propIndex = propsToColsMap.Count;
					propsToColsMap.Add(p.PropertyType, propIndex);

					MakeValue(headerItem, "pn" + (propIndex + 1), p.PropertyType.FullName);
				}
				if (propIndex > MAX_PROPS) continue;

				//Only numbers, because excel has date format and dates parsed in excel only partially
				var typify = p.PropertyType.ValueType == CustomPropertyValueType.Number;
				MakeValue(rowItem, "p" + (propIndex + 1), p.Value.ToString(), typify);
			}
		}
	}
}
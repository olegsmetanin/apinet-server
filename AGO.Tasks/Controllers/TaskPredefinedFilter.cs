using System;
using AGO.Core.Filters;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Controllers
{
	/// <summary>
	/// Предустановленные фильтры
	/// </summary>
	public enum TaskPredefinedFilter
	{
		All,
		Overdue,
		DayLeft,
		WeekLeft,
		NoLimit,
		ClosedToday,
		ClosedYesterday
	}

	public static class TaskPredefinedFilterExtensions
	{
		/// <summary>
		/// Превращает значение предустановленного фильтра в критерий
		/// </summary>
		/// <param name="predefined">Значение фильтра</param>
		/// <param name="builder">Построитель критерия</param>
		/// <returns>Фильтр или null, если фильтр не задан</returns>
		public static IModelFilterNode ToFilter(
			this TaskPredefinedFilter predefined, IModelFilterBuilder<TaskModel, TaskModel> builder)
		{
			IModelFilterNode predefinedPredicate = null;
			if (predefined != TaskPredefinedFilter.All)
			{
				var today = DateTime.Today;

				switch (predefined)
				{
					case TaskPredefinedFilter.Overdue:
						{
							var tomorrow = today.AddDays(1);
							predefinedPredicate = builder.Where(m => m.Status != TaskStatus.Closed && m.DueDate < tomorrow);
						}
						break;
					case TaskPredefinedFilter.DayLeft:
						{
							var dayAfterTomorrow = today.AddDays(2);
							predefinedPredicate = builder.Where(m => m.Status != TaskStatus.Closed && m.DueDate < dayAfterTomorrow);
						}
						break;
					case TaskPredefinedFilter.WeekLeft:
						{
							var weekLater = today.AddDays(8);
							predefinedPredicate = builder.Where(m => m.Status != TaskStatus.Closed && m.DueDate < weekLater);
						}
						break;
					case TaskPredefinedFilter.NoLimit:
						predefinedPredicate = builder.WhereProperty(m => m.DueDate).Not().Exists();
						break;
					case TaskPredefinedFilter.ClosedToday:
						{
							var tomorrow = today.AddDays(1);
							predefinedPredicate = builder.And()
								.Where(m => m.Status == TaskStatus.Closed)
								.WhereCollection(m => m.StatusHistory)
								.Where(m => m.Status == TaskStatus.Closed && m.Start >= today && m.Start < tomorrow).End();
						}
						break;
					case TaskPredefinedFilter.ClosedYesterday:
						var yesterday = today.AddDays(-1);
						predefinedPredicate = builder.And()
							.Where(m => m.Status == TaskStatus.Closed)
							.WhereCollection(m => m.StatusHistory)
							.Where(m => m.Status == TaskStatus.Closed && m.Start >= yesterday && m.Start < today).End();
						break;
					default:
						throw new ArgumentOutOfRangeException("predefined");
				}
			}
			return predefinedPredicate;
		}
	}
}
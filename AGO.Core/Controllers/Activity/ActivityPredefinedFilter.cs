using System;
using System.Threading;
using AGO.Core.Filters;
using AGO.Core.Model.Activity;

namespace AGO.Core.Controllers.Activity
{
	/// <summary>
	/// Предустановленные фильтры
	/// </summary>
	public enum ActivityPredefinedFilter
	{
		Today,
		Yesterday,
		ThisWeek,
		PastWeek,
		ThisMonth,
		PastMonth
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
			this ActivityPredefinedFilter predefined, IModelFilterBuilder<ActivityRecordModel, ActivityRecordModel> builder)
		{
			var today = DateTime.Today;
			var culture = Thread.CurrentThread.CurrentUICulture;

			if (predefined == ActivityPredefinedFilter.Today)
			{
				var tomorrow = today.AddDays(1);
				return builder.Where(m => m.CreationTime >= today && m.CreationTime < tomorrow);
			}
			
			if (predefined == ActivityPredefinedFilter.Yesterday)
			{
				var yesterday = today.AddDays(-1);
				return builder.Where(m => m.CreationTime >= yesterday && m.CreationTime < today);
			}

			if (predefined == ActivityPredefinedFilter.ThisWeek || predefined == ActivityPredefinedFilter.PastWeek)
			{
				var firstDay = today;
				while (firstDay.DayOfWeek != culture.DateTimeFormat.FirstDayOfWeek)
					firstDay = firstDay.AddDays(-1);

				if (predefined == ActivityPredefinedFilter.PastWeek)
					firstDay = firstDay.AddDays(-7);

				var nextFirstDay = firstDay.AddDays(7);

				return builder.Where(m => m.CreationTime >= firstDay && m.CreationTime < nextFirstDay);
			}

			if (predefined == ActivityPredefinedFilter.ThisMonth || predefined == ActivityPredefinedFilter.PastMonth)
			{
				var firstDay = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Local);

				if (predefined == ActivityPredefinedFilter.PastMonth)
					firstDay = firstDay.AddMonths(-1);

				var nextFirstDay = firstDay.AddMonths(1);

				return builder.Where(m => m.CreationTime >= firstDay && m.CreationTime < nextFirstDay);
			}
			
			throw new ArgumentOutOfRangeException("predefined");
		}
	}
}
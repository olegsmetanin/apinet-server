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
}
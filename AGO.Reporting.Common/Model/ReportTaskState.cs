namespace AGO.Reporting.Common.Model
{
	/// <summary>
	/// Состояние задачи по созданию отчета
	/// </summary>
	public enum ReportTaskState
	{
		/// <summary>
		/// Задача создана, но еще не выполняется
		/// </summary>
		NotStarted = 0,

		/// <summary>
		/// Задача выполняется
		/// </summary>
		Running = 1,

		/// <summary>
		/// Задача выполнена успешно
		/// </summary>
		Completed = 2,

		/// <summary>
		/// Задача отменена пользователем
		/// </summary>
		Canceled = 3,

		/// <summary>
		/// Выполнение прекращено из-за ошибки
		/// </summary>
		Error = 4
	}
}
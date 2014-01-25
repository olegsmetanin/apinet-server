namespace AGO.Reporting.Common
{
	/// <summary>
	/// Строковые идентификаторы событий, отсылаемых клиенту при изменении задачи на создание отчета
	/// </summary>
	public static class ReportEvents
	{
		public static readonly string CREATED = "created";
		public static readonly string RUNNED = "runned";
		public static readonly string PROGRESS = "progress";
		public static readonly string COMPLETED = "completed";
		public static readonly string ABORTED = "aborted";
		public static readonly string ERROR = "error";
		public static readonly string CANCELED = "canceled";
		public static readonly string DELETED = "deleted";
		public static readonly string DOWNLOADED = "downloaded";
	}
}
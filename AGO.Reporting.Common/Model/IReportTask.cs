using System;

namespace AGO.Reporting.Common.Model
{
	/// <summary>
	/// Задача на создание отчета
	/// </summary>
	public interface IReportTask: IReportingModel
	{
		#region Settings

		/// <summary>
		/// Настройка отчета, который создается по этой задаче
		/// </summary>
		IReportSetting Setting { get; }

		/// <summary>
		/// Сериализованное (json) представление параметров отчета
		/// </summary>
		string Parameters { get; }

		/// <summary>
		/// Логин автора
		/// </summary>
		Guid AuthorId { get; }

		/// <summary>
		/// Идентификатор языковых настроек, используемых при генерации отчета (названия enum-ов, числа, даты и т.п.)
		/// </summary>
		string Culture { get; }

		#endregion

		#region State

		/// <summary>
		/// Текущее состояние задачи
		/// </summary>
		ReportTaskState State { get; set; }

		/// <summary>
		/// % выполнения задачи генерации данных для отчета
		/// </summary>
		byte DataGenerationProgress { get; set; }

		/// <summary>
		/// % выполнения задачи создания отчета по данным
		/// </summary>
		byte ReportGenerationProgress { get; set; }

		/// <summary>
		/// Время начала создания отчета
		/// </summary>
		DateTime? StartedAt { get; set; }

		/// <summary>
		/// Время завершения создания отчета (по любой причине, в т.ч. ошибка или отмена)
		/// </summary>
		DateTime? CompletedAt { get; set; }

		/// <summary>
		/// Сообщение об ошибке (если она произошла)
		/// </summary>
		string ErrorMsg { get; set; }

		/// <summary>
		/// Информация об ошибке (stack trace и прочие данные)
		/// </summary>
		string ErrorDetails { get; set; }

		#endregion

		#region Result

		/// <summary>
		/// Результат (файл отчета)
		/// </summary>
		byte[] ResultContent { get; set; }

		/// <summary>
		/// Результат (имя файла)
		/// </summary>
		string ResultName { get; set; }

		/// <summary>
		/// Результат (MIME-type)
		/// </summary>
		string ResultContentType { get; set; }

		#endregion
	}
}
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
		/// Сервис, на котором выполняется задача
		/// </summary>
		IReportingServiceDescriptor Service { get; }

		/// <summary>
		/// Сериализованное (json) представление параметров отчета
		/// </summary>
		string Parameters { get; }

		#endregion

		#region State

		/// <summary>
		/// Текущее состояние задачи
		/// </summary>
		ReportTaskState State { get; set; }

		/// <summary>
		/// % выполнения задачи генерации данных для отчета
		/// </summary>
		byte DateGenerationProgress { get; }

		/// <summary>
		/// % выполнения задачи создания отчета по данным
		/// </summary>
		byte ReportGenerationProgress { get; }

		/// <summary>
		/// Время начала создания отчета
		/// </summary>
		DateTime? StartedAt { get; }

		/// <summary>
		/// Время завершения создания отчета (по любой причине, в т.ч. ошибка или отмена)
		/// </summary>
		DateTime? CompletedAt { get; }

		/// <summary>
		/// Сообщение об ошибке (если она произошла)
		/// </summary>
		string ErrorMsg { get; }

		/// <summary>
		/// Информация об ошибке (stack trace и прочие данные)
		/// </summary>
		string ErrorDetails { get; }

		#endregion

		#region Result

		/// <summary>
		/// Результат (файл отчета)
		/// </summary>
		byte[] Result { get; set; }

		/// <summary>
		/// MIME-type результата
		/// </summary>
		string ResultContentType { get; set; }

		#endregion
	}
}
using System;

namespace AGO.Reporting.Common.Model
{
	/// <summary>
	/// Базовый класс для моделей сервиса отчетов
	/// </summary>
	public interface IReportingModel
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// Наименование
		/// </summary>
		string Name { get; }
	}
}
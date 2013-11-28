using System;

namespace AGO.Reporting.Common.Model
{
	/// <summary>
	/// Шаблон отчета
	/// </summary>
	public interface IReportTemplate: IReportingModel
	{
		/// <summary>
		/// Содержимое шаблона отчета
		/// </summary>
		byte[] Content { get; }

		/// <summary>
		/// Дата последнего обновления шаблона
		/// </summary>
		/// <remarks>Используется для кеширования шаблонов на диске</remarks>
		DateTime LastChange { get; }
	}
}
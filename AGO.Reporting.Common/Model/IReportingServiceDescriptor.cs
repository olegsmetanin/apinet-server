using System;

namespace AGO.Reporting.Common.Model
{
	/// <summary>
	/// Настройки экземпляра сервиса отчетов
	/// </summary>
	public interface IReportingServiceDescriptor: IReportingModel
	{
		/// <summary>
		/// Uri-адрес апи сервиса
		/// </summary>
		string EndPoint { get; }

		/// <summary>
		/// Признак, что сервис поддерживает генерацию долго выполняющихся отчетов
		/// </summary>
		bool LongRunning { get; }
	}
}

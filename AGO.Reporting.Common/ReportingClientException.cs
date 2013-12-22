using System;

namespace AGO.Reporting.Common
{
	/// <summary>
	/// Исключение при работе с клиентом сервиса отчетов
	/// </summary>
	public class ReportingClientException: Exception
	{
		public ReportingClientException(string message): base(message)
		{
		}

		public ReportingClientException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
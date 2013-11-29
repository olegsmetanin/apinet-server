using System.Xml;

namespace AGO.Reporting.Common
{
	/// <summary>
	/// Генератор данных для отчетов
	/// </summary>
    public interface IReportDataGenerator
    {
        XmlDocument GetReportData(string reportParams);
    }
}
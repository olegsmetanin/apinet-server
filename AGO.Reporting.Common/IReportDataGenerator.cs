using System.Xml;

namespace AGO.Reporting.Common
{
	/// <summary>
	/// ��������� ������ ��� �������
	/// </summary>
    public interface IReportDataGenerator
    {
        XmlDocument GetReportData(string reportParams);
    }
}
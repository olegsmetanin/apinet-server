using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using AGO.Reporting.Common;

namespace AGO.Reporting.Service.ReportGenerators
{
    /// <summary>
    /// ��������� ������� � ������� Rtf.
    /// </summary>
    public class RtfReportGenerator : BaseReportGenerator, IReportGenerator
    {
        private static readonly Regex MarkerMatcher = new Regex(@"\</[\w,\.]+\>", RegexOptions.Compiled);

        private const string MARKER_START_SYMBOL = "</";
        private const string MARKER_END_SYMBOL = ">";

        #region IReportGenerator Members
        private string reportName;
        private string templateName;
        private StringBuilder reportBuilder;


        protected override string MarkerStartSymbol
        {
            get { return MARKER_START_SYMBOL; }
        }

        protected override string MarkerEndSymbol
        {
            get { return MARKER_END_SYMBOL; }
        }

        public void MakeReport(string pathToTemplate, XmlDocument data, CancellationToken token)
        {
        	CancellationToken = token;
            reportBuilder = new StringBuilder();
            using(StreamReader sr = File.OpenText(pathToTemplate))
            {
                reportBuilder.Append(sr.ReadToEnd());
                sr.Close();
            }

            //���������� ��� �������, ��� �� �� ���� ����� ������� ��� �����
            templateName = Path.GetFileName(pathToTemplate);
            //�������� �������� ��� ����� �� ������ ���������� ������ � ������������ ���, ����
            //������� ��� ����� ������ �� ������.
            string dataGenFileName = data.DocumentElement.GetAttribute("reportName");
            if (string.IsNullOrEmpty(reportName) && !string.IsNullOrEmpty(dataGenFileName))
                reportName = dataGenFileName;

            XmlNodeList ranges = data.DocumentElement.SelectNodes(XPATH_RANGES);
            InitTicker(ranges.Count);
            foreach (XmlNode range in ranges)
            {
                XmlNodeList rangeItems = range.SelectNodes(XPATH_ITEMS);
                Ticker.Ticks += rangeItems.Count;
                foreach (XmlNode rangeItem in rangeItems)
                {
                    XmlNodeList itemValues = rangeItem.SelectNodes(XPATH_VALUES);
                    Ticker.Ticks += itemValues.Count;
                    foreach (XmlNode itemValue in itemValues)
                    {
                        string markerName = itemValue.SelectSingleNode(XPATH_NAME).Value;
                        string value = itemValue.InnerText;
                        ReplaceMarker(markerName, value);
                        Ticker.AddTick();
                    }
                    Ticker.AddTick(); //��� ��� �����
                }
                Ticker.AddTick(); //��� ��� ���������
            }

            ClearAllMarkers();
        }

        /// <summary>
        /// �������� ������ � rtf-������� ��������� ���������.
        /// </summary>
        /// <param name="markerName">��� ������� (��� ����.�������� �� �����).</param>
        /// <param name="value">��������, �� ������� ���� �������� ������.</param>
        private void ReplaceMarker(string markerName, string value)
        {
            string marker = CompleteMarker(markerName);
            while (reportBuilder.ToString().Contains(marker))
            {
                reportBuilder.Replace(marker, value);
            }
        }

        /// <summary>
        /// ���������, �������� �� rtf-������ ��������� ������ ��� ������ �������.
        /// </summary>
        /// <param name="markers">������ ��������, ������������ � ������� ������.</param>
        /// <returns>true, ���� ������ ������ �������� ������(�).</returns>
        private bool HasMarker(out string[] markers)
        {
            markers = new string[0];
            string reportText = reportBuilder.ToString();
            if (string.IsNullOrEmpty(reportText)) return false;
            bool has = !string.IsNullOrEmpty(reportText) &&
                       reportText.Contains(MarkerStartSymbol) &&
                       reportText.Contains(MarkerEndSymbol);
            if (has)
            {
                MatchCollection matches = MarkerMatcher.Matches(reportText);
                markers = new string[matches.Count];
                for (int i = 0; i < matches.Count; i++)
                {
                    markers[i] = matches[i].Value;
                }
            }
            return has;
        }

        /// <summary>
        /// ���� ������� � rtf-������� ��������� � �������� �� �� ������ ������.
        /// </summary>
        private void ClearAllMarkers()
        {
            string[] markers;
            if (HasMarker(out markers))
            {
                foreach (string marker in markers)
                {
                    ReplaceMarker(marker, string.Empty);
                }
            }
        }
        #endregion

        #region IReportGeneratorResult Members

    	public Stream Result
        {
            get
            {
                var ms = new MemoryStream();
                byte[] reportContent = Encoding.UTF8.GetBytes(reportBuilder.ToString());
                ms.Write(reportContent, 0, reportContent.Length);
                ms.Position = 0;
                return ms;
            }
        }

        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(reportName))
                {
                    reportName = Path.ChangeExtension(templateName, "rtf");
                }
                if (!reportName.EndsWith("rtf"))
                {
                    reportName = reportName + ".rtf";
                }
                return reportName;
            }
        }

    	public string ContentType
    	{
			get { return "text/rtf"; }
    	}

    	#endregion
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using AGO.Reporting.Common;

namespace AGO.Reporting.Service.ReportGenerators
{
    /// <summary>
    /// Каркас для генератора отчетов в формате Comma-separated value (Csv)
    /// </summary>
    public class CsvReportGenerator : BaseReportGenerator, IReportGenerator
    {
        private string templateFileName;
        private string reportFileName;
        private StringBuilder report;

    	private const string BEGIN_RANGE = "<range_{0}>";
        private const string END_RANGE = "</range_{0}>";

        private readonly Regex MARKER_REGEX = new Regex("{[^{]+?}");

        #region IReportGenerator Members

        public void MakeReport(string pathToTemplate, XmlDocument data, CancellationToken token)
        {
        	CancellationToken = token;

            if (data == null || data.DocumentElement == null)
            {
                throw new ArgumentException("Не заданы данные для генерации отчета (data).");
            }

            report = new StringBuilder();
            var template = File.ReadAllText(pathToTemplate);

            //Запоминаем имя шаблона, что бы из него потом сделать имя файла
            templateFileName = Path.GetFileName(pathToTemplate);

            //Пытаемся получить имя файла из данных генератора данных и использовать его, если
            //снаружи имя файла отчета не задано.
            var dataGenFileName = data.DocumentElement.GetAttribute("reportName");
            if (string.IsNullOrEmpty(reportFileName) && !string.IsNullOrEmpty(dataGenFileName))
                reportFileName = dataGenFileName;


            var ranges = data.DocumentElement.SelectNodes(XPATH_RANGES);
            if (ranges == null || ranges.Count <= 0)
            {
                //Диапазонов данных нет, значит и выводить нечего.
                return;
            }
            InitTicker(ranges.Count);

            var contentRanges = new Dictionary<string, StringBuilder>();
            foreach (XmlNode rangeNode in ranges)
            {
                string currentRangeName = rangeNode.SelectSingleNode(XPATH_NAME).Value;
                if (!RangeExists(template, currentRangeName)) continue;

                XmlNodeList items;
                int itemCount = (items = rangeNode.SelectNodes(XPATH_ITEMS)) != null ? items.Count : 0;

                if (itemCount==0)
                {
                    ClearAllMarkers(currentRangeName, ref template);
                    Ticker.Ticks += 1;
                    Ticker.AddTick();
                    continue;
                }
                
                FillRange(currentRangeName, rangeNode, ref template, ref contentRanges);
                Ticker.Ticks += 1;
                Ticker.AddTick();
                
                Ticker.AddTick();
            }

            FillReport(contentRanges, template);
        }

        private void FillReport(Dictionary<string, StringBuilder> contentRanges, string template)
        {
            report = new StringBuilder();

            var keys = new List<KeyValuePair<int, string>>();
            foreach (var key in contentRanges.Keys)
            {
                var index = template.IndexOf(key);
                if (index==-1)
                {
                    throw new ReportingException(string.Format("Не найден ключ '{0}'. Обратитесь к разработчикам.", index));
                }

                keys.Add(new KeyValuePair<int, string>(index, key));
            }

            var leftLimit = 0;
            foreach (var keyValuePair in keys.OrderBy(k => k.Key))
            {
                if (keyValuePair.Key != leftLimit)
                {
                    report.Append(template.Substring(leftLimit, keyValuePair.Key - leftLimit));
                }

                report.Append(contentRanges[keyValuePair.Value].ToString());
                leftLimit = keyValuePair.Key + keyValuePair.Value.Length;
            }
            if (leftLimit < template.Length)
            {
                report.Append(template.Substring(leftLimit, template.Length - leftLimit - 1));
            }
        }

        private bool RangeExists(string template, string currentRangeName)
        {
            var startRangeName = string.Format(BEGIN_RANGE, currentRangeName);
            var endRangeName = string.Format(END_RANGE, currentRangeName);

            var startRange = template.IndexOf(startRangeName);
            var endRange = template.IndexOf(endRangeName);

            if (startRange==-1 || endRange==-1)
            {
                return false;
            }

            if (startRange > endRange)
            {
                throw new ReportingException(string.Format("Ошибка в границах диапазона '{0}'", currentRangeName));
            }

            return true;
        }

        private void ClearAllMarkers(string currentRangeName, ref string template)
        {
            var startRangeName = string.Format(BEGIN_RANGE, currentRangeName);
            var endRangeName = string.Format(END_RANGE, currentRangeName);

            var startRange = template.IndexOf(startRangeName);
            var endRange = template.IndexOf(endRangeName);

            template = template.Remove(startRange, endRange + endRangeName.Length - startRange);
        }

        private string GetUniqueKeyRange(string rangeName)
        {
            return string.Format("[{0}_{1}]", rangeName, Guid.NewGuid());
        }

        private void FillRange(string currentRangeName, XmlNode rangeNode, ref string template, ref Dictionary<string, StringBuilder> contentRanges)
        {
            var startRangeName = string.Format(BEGIN_RANGE, currentRangeName);
            var endRangeName = string.Format(END_RANGE, currentRangeName);

            var startRange = template.IndexOf(startRangeName);
            var endRange = template.IndexOf(endRangeName);

            var uniqueRangeKey = GetUniqueKeyRange(currentRangeName);
            var rangeTemplate = template.Substring(startRange + startRangeName.Length, endRange - startRange - startRangeName.Length);

            var content = new StringBuilder();

            foreach (XmlNode rangeItem in rangeNode.ChildNodes)
            {
                var item = rangeTemplate;
                foreach (XmlNode valueNode in rangeItem.ChildNodes)
                {
                    string marker = CompleteMarker(valueNode.SelectSingleNode(XPATH_NAME).Value);
                    string value = valueNode.InnerText;
                    // TODO: Определить нужен ли typify
                    // bool typify = bool.Parse(valueNode.SelectSingleNode(XPATH_TYPIFY).Value);
                    item = item.Replace(marker, string.IsNullOrEmpty(value) ? value : value.Replace('"', '\''));
                }
                
                item = MARKER_REGEX.Replace(item, string.Empty);
                if (rangeNode.ChildNodes.Count > 1)
                    content.AppendLine(item);
                else
                    content.Append(item);
            }
            template = template.Remove(startRange + startRangeName.Length, endRange + endRangeName.Length - startRangeName.Length - startRange);
            template = template.Replace(startRangeName, uniqueRangeKey);
            contentRanges.Add(uniqueRangeKey, content);
        }


        #endregion

        #region IReportGeneratorResult Members

        public Stream Result
        {
            get
            {
                string myString = report.ToString();
                //byte[] BOM = { 0xEF, 0xBB, 0xBF };
                byte[] myByteArray = Encoding.UTF8.GetBytes(myString);
            	byte[] allBytes = myByteArray;// BOM.Concat(myByteArray).ToArray();

                return new MemoryStream(allBytes);
            }
        }

        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(reportFileName))
                {
                    reportFileName = Path.ChangeExtension(templateFileName, "csv");
                }
                if (!reportFileName.EndsWith("csv"))
                {
                    reportFileName = reportFileName + ".csv";
                }
                return reportFileName;
            }
        }

    	public string ContentType
    	{
			get { return "text/csv"; }
    	}

    	#endregion
    }
}
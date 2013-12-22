using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using AGO.Reporting.Common;
using Syncfusion.XlsIO;

namespace AGO.Reporting.Service.ReportGenerators
{
    /// <summary>
    /// ��������� ������� �� XML-���������� ��������� ������� (������������ � Excel � �������
    /// SyncFusion Xlsio).
    /// </summary>
    public class SyncFusionXlsReportGenerator: BaseReportGenerator, IReportGenerator
    {
        private static readonly Regex MarkerMatcher = new Regex(@"\{\$[\w,\.]+\$\}", RegexOptions.Compiled);

        private string templateFileName;
        private string reportFileName;

        #region IReportGeneratorResult
        public Stream Result
        {
            get
            {
                var ms = new MemoryStream();
                report.SaveAs(ms, ExcelSaveType.SaveAsXLS);
                ms.Position = 0;
                return ms;
            }
        }

        public string GetFileName(string proposed)
        {
			//priority: user proposed, data gen proposed, template name
			if (!string.IsNullOrWhiteSpace(proposed))
			{
				reportFileName = proposed;
			} 
			else if (string.IsNullOrEmpty(reportFileName))
            {
                reportFileName = templateFileName;
            }

			return Path.ChangeExtension(reportFileName, "xls");
        }

    	public string ContentType
    	{
			get { return "application/vnd.ms-excel"; }
    	}

    	#endregion

        #region ������ ���������� ������

        private ExcelEngine engine;
        private IWorkbook report;

        public IWorkbook Report
        {
            get { return report; }
        }

        public void MakeReport(string pathToTemplate, XmlDocument data, CancellationToken token)
        {
        	CancellationToken = token;
            if (data == null || data.DocumentElement == null)
            {
                throw new ArgumentException("�� ������ ������ ��� ��������� ������ (data).");
            }

            engine = new ExcelEngine {ThrowNotSavedOnDestroy = false};
            report = engine.Excel.Workbooks.Open(pathToTemplate);

            //���������� ��� �������, ����� �� ���� ����� ������� ��� �����
            templateFileName = Path.GetFileName(pathToTemplate);
            //�������� �������� ��� ����� �� ������ ���������� ������ � ������������ ���, ����
            //������� ��� ����� ������ �� ������.
            string dataGenFileName = data.DocumentElement.GetAttribute("reportName");
            if (string.IsNullOrEmpty(reportFileName) && !string.IsNullOrEmpty(dataGenFileName))
                reportFileName = dataGenFileName;


            var ranges = data.DocumentElement.SelectNodes(XPATH_RANGES);
            if (ranges == null || ranges.Count <= 0)
            {
                //���������� ������ ���, ������ � �������� ������.
                return;
            }
            InitTicker(ranges.Count);
            foreach (XmlNode rangeNode in ranges)
            {
                //������� �������� ������������� �����, ��� �������� ������������ �������� ������
                IWorksheet sheet;
                if (!SheetExists(report, rangeNode.SelectSingleNode(XPATH_SHEETNAME).Value, out sheet)) continue;

                //���� ����� ����. ������ ���������, ���� �� ��� �������� ���������� ��������
                IRange range;
                string currentRangeName = rangeNode.SelectSingleNode(XPATH_NAME).Value;
                if (!RangeExists(sheet, currentRangeName, out range)) continue;

                //���� � �������� �������. ���� ��������� ������. ��� ����� ������������ 2 ���������:
                //- ������� ����������, �.�. � �������� <range> ��������� ������ ���� item, � ���� ������
                //  ��������� ��� �������� �� ������
                //- ����������� �����, �.�. ���� �� ������ ��������� �������� �� ������, �� � ������������
                //  �������� �������� ������ ���-�� ���
                XmlNodeList items;
                int itemCount = (items = rangeNode.SelectNodes(XPATH_ITEMS)) != null ? items.Count : 0;
                switch (itemCount)
                {
                    case 0:
                        //�������. � �������� <range> ������ ���. �� �� ��� � ���� ���. ��� �� �����
                        //��� �������� ����������. ���� ������ ������ ���, �� ��������, ��� ����� �������
                        //���������, ��� ������ ����� �� ���������, ��� �� ��� �� ������� ������� ��� ������
                        ClearAllMarkers(range);
                        Ticker.Ticks += 1;
                        Ticker.AddTick();
                        break;
                    case 1:
                        Ticker.Ticks += 2;
                        //���� ��������� ������� �� ����� ������
                        FillValuesInRange(range, rangeNode.FirstChild);
                        Ticker.AddTick();
                        //� ������� �������, ���� �������� �� �����������
                        ClearAllMarkers(range);
                        Ticker.AddTick();
                        break;
                    default:
                        //��������� �������� ������� � �������������� ��� ������ ���-�� ���
                        IRange currentRange = range;

                        int startRow = currentRange.Row;
                        int endRow = currentRange.LastRow;
                        int startCol = currentRange.Column;
                        int endCol = currentRange.LastColumn;
                        int rangeSizeInRows = currentRange.LastRow - startRow;
                        bool needCopy = startCol == 1;
                        Ticker.Ticks += rangeNode.ChildNodes.Count * 2;

                        //var rangesToShift = new List<IRange>();

                        foreach (XmlNode itemNode in rangeNode.ChildNodes)
                        {
                            //���� ���������� �� ��������� �������, �� ��������� ������ �� ����� �����������
                            //�� ���� ���� ������. ���������� ������ ����� ������ ���� ��� ������� �� ���� ����.
                            //���� ����� � �������� ���� ������, �� ��� ���������� ����� ��� ���� �������� ����.
                            //������� ���� ������������� �� ����� ��������� � ������ �������. ��������, ��������
                            //� ������� ������� ��� ��������� �������������
                            if (itemNode != rangeNode.LastChild)
                            {
                                startRow += rangeSizeInRows + 1;
                                endRow += rangeSizeInRows + 1;
                                //���� ��� ����������� ��������� ����� ����������� ������������ ������, ��
                                //����������� �� �������� (� �����������)
                                //������� �������������� ��������� ���� ����������� ��������� ������ ���-�� �����
                                //� ������� � ��� �����������
                                if (needCopy)
                                {
                                    sheet.InsertRow(startRow, currentRange.Rows.Length, ExcelInsertOptions.FormatDefault);
                                }
                                currentRange.CopyTo(sheet.Range[startRow, startCol, endRow, endCol], ExcelCopyRangeOptions.All);
                                //���������� � ����� �������� ������ �����, ��� �� �� �������� ������� ��������������.
                                for (var i = 0; i < currentRange.Rows.Length; i++)
                                {
                                    sheet.Rows[startRow + i - 1].RowHeight = sheet.Rows[currentRange.Row + i - 1].RowHeight;
                                }
                            }
                            //������� ��������� �������. ��� ���� ��� ��� ������, ��� ������� �� ������� ��������,
                            //� ������ ������ �������� �� �����
                            FillValuesInRange(currentRange, itemNode);
                            Ticker.AddTick();
                            //�� � ������ ����� �������� �������, ������� �� ������ � ������. � ����� ��� ���������
                            //�� �����, ��� ��������. ������� ���������� ��������� ����� ����.
                            ClearAllMarkers(currentRange);
                            Ticker.AddTick();

                            if (itemNode != rangeNode.LastChild)
                            {
                                currentRange = sheet.Range[startRow, startCol, endRow, endCol];
                            }
                        }
                        break;
                }
                Ticker.AddTick();
                //����� ������ ������ ������-�� ����������� ��������� ���������� �������������
                //������, ���� ���� ������ ����� ������������� � ������.
                Groups.MergeAndClear(sheet);
            }
        }

        /// <summary>
        /// ��� ������� ���������� �������� value � item ���������� ����� ������� � ������
        /// �� �������� name � ��������� �� �������� (CDATA ������)
        /// </summary>
        /// <param name="range">�������� ����� ��� ������</param>
        /// <param name="item">������� ������, ���������� � ���� ������ ��������</param>
        private void FillValuesInRange(IRange range, XmlNode item)
        {
            foreach (XmlNode valueNode in item.ChildNodes)
            {
                string marker = CompleteMarker(valueNode.SelectSingleNode(XPATH_NAME).Value);
                string value = valueNode.InnerText;
                bool typify = bool.Parse(valueNode.SelectSingleNode(XPATH_TYPIFY).Value);
                XmlNode groupNode;
                string group = (groupNode = valueNode.SelectSingleNode(XPATH_GROUP)) != null ? groupNode.Value : null;
                ReplaceMarker(range, marker, value, typify, group);
            }
        }

        /// <summary>
        /// ���� ������� � �������� ��������� � ������� � �����
        /// </summary>
        /// <param name="range">�������� ����� ����� ��� ������</param>
        private void ClearAllMarkers(IRange range)
        {
            foreach (var cell in range.Cells)
            {
                string[] markers;
                if (HasMarker(cell.Value, out markers))
                {
                    foreach (string marker in markers)
                    {
                        ReplaceMarker(cell, marker, string.Empty, false);
                    }
                }
            }
        }

        /// <summary>
        /// ���������, �������� �� �������� ������ ������ ��� ������ �������
        /// </summary>
        /// <param name="cellValue">�������� ������</param>
        /// <param name="markers">������ ��������, ������������ � �������� ������</param>
        /// <returns>true, ���� �������� ������ �������� ������(�)</returns>
        private bool HasMarker(object cellValue, out string[] markers)
        {
            markers = new string[0];
            if (cellValue == null) return false;
            string strCellValue = cellValue.ToString();
            bool has = !string.IsNullOrEmpty(strCellValue) &&
                       strCellValue.Contains(MarkerStartSymbol) &&
                       strCellValue.Contains(MarkerEndSymbol);
            if (has)
            {
                MatchCollection matches = MarkerMatcher.Matches(strCellValue);
                markers = new string[matches.Count];
                for (int i = 0; i < matches.Count; i++)
                {
                    markers[i] = matches[i].Value;
                }
            }
            return has;
        }

        /// <summary>
        /// ����� ������� � ��������� ����� (��� �������, ���������� �� ������������ ���������)
        /// </summary>
        /// <param name="where">�������� ����� ����� ��� ������</param>
        /// <param name="marker">������</param>
        /// <param name="value">�������� ��� ������</param>
        /// <param name="typify">�������, �������� ��� ��� ������������ �������� <paramref name="value"/>.</param>
        /// <param name="group">��� ������, � ������� ��������� �������� ������ (��� ����������� �����).</param>
        private void ReplaceMarker(IRange where, string marker, object value, bool typify, string group)
        {
            List<IRange> matchedRanges = null;
            foreach (var cell in where.Cells)
            {
                if (cell.Value.Contains(marker))
                {
                    if (matchedRanges == null) matchedRanges = new List<IRange>();
                    matchedRanges.Add(cell);
                }
            }

            //�������, �� SyncFusion �� ����� ������ ����� �� �������� ������, �������� ������ ������ (��. ����).
            //var matchedRanges = where.FindAll(marker, ExcelFindType.Text | ExcelFindType.Formula);

            if (matchedRanges == null || matchedRanges.Count == 0) return;
            foreach (var range in matchedRanges)
            {
                //������������ ��� ������, ��� �� ����� ������������� ��.
                if (!string.IsNullOrEmpty(group))
                {
                    Groups.RegisterCell(group, range.Row, range.Column);
                }

                ReplaceMarker(range, marker, value, typify);
            }
        }

        /// <summary>
        /// �������� ������ � ������ ������ ���������. ��������� �������� �������� �������������,
        /// ��� �����/������� �����, ����.
        /// </summary>
        /// <param name="where">������ �����, � ������� ����� ���������� ������</param>
        /// <param name="marker">������</param>
        /// <param name="value">�������� ��� ������</param>
        /// <param name="typify">�������, �������� ��� ��� ������������ �������� <paramref name="value"/>.</param>
        private static void ReplaceMarker(IRange where, string marker, object value, bool typify)
        {
            var strValue = Convert.ToString(value);

            //���� ���������� � =, �� ��� �������.
            if (strValue.StartsWith("="))
            {
                where.Formula = Convert.ToString(value);
                return;
            }

            string cellValue = where.Value;
            cellValue = cellValue.Replace(marker, FixLongOrNullString(Convert.ToString(value)));

            if (!typify)
            {
                where.Text = cellValue;
                return;
            }

            int intValue;
            if (int.TryParse(cellValue, out intValue))
            {
                where.Number = intValue;
                return;
            }
            double doubleValue;
            if (double.TryParse(cellValue, out doubleValue))
            {
                where.Number = doubleValue;
                return;
            }
            DateTime dateValue;
            if (DateTime.TryParseExact(cellValue, "dd.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateValue))
            {
                where.DateTime = dateValue;
                return;
            }
            where.Value = cellValue;
        }

        /// <summary>
        /// ������� ������� ������ ������ ������ �����.
        /// ������ ������ 994 �������� ����� ��������.
        /// </summary>
        /// <param name="value">��������� �������� ��� �������</param>
        /// <returns>�������� ��� ���������� ������</returns>
        protected static string FixLongOrNullString(string value)
        {
            if (value == null) return string.Empty;
            return value.Length <= 994 ? value : value.Substring(0, 990) + "...";
        }

        /// <summary>
        /// ��������� ������� (� ����������) ����� �� ����� �� �����.
        /// <remarks>���� ������ ���������� � ����� �� �����, � ��� �� ���������� �������, ��
        /// ��������� ����������.</remarks>
        /// </summary>
        /// <param name="file">����������� �� ������� ����</param>
        /// <param name="sheetName">��� �����</param>
        /// <param name="sheet">��������� ���� ��� null, ���� �� �������</param>
        /// <returns>true, ���� ���� ������ � �����</returns>
        private static bool SheetExists(IWorkbook file, string sheetName, out IWorksheet sheet)
        {
            sheet = file.Worksheets[sheetName];
            return sheet != null;
        }

        /// <summary>
        /// ��������� ������� (� ����������) ������������ ��������� �� �����.
        /// <remarks>���� ������ ���������� � ��������� �� �����, � ��� �� ����������, �� ��������� null.
        /// ����� ���� �� ������ ������������ null, �� ����� ��������������� � ������� �� �������� � 
        /// ��������� ������������� ����� � �����.</remarks>
        /// </summary>
        /// <param name="sheet">���� �����</param>
        /// <param name="rangeName">��� ���������</param>
        /// <param name="range">��������� �������� ��� null, ���� �� �������</param>
        /// <returns>true, ���� �������� ������</returns>
        private static bool RangeExists(IWorksheet sheet, string rangeName, out IRange range)
        {
            range = sheet.Range[rangeName];
            return range != null;
        }
        #endregion

        #region ��������������� ����

        private readonly GroupsMgr Groups = new GroupsMgr();

        /// <summary>
        /// ��������������� ����� ��� ����������� �����.
        /// </summary>
        private class GroupsMgr
        {
            private readonly Dictionary<string, CellsGroup> _groups = new Dictionary<string, CellsGroup>();

            public void RegisterCell(string group, int row, int column)
            {
                if (!_groups.ContainsKey(group))
                {
                    var cg = new CellsGroup(group, row, column, row, column);
                    _groups.Add(cg.GroupName, cg);
                }
                else
                {
                    var gr = _groups[group];
                    gr.RegisterCell(row, column);
                }
            }

            public void MergeAndClear(IWorksheet sheet)
            {
                foreach (var group in _groups.Keys)
                {
                    _groups[group].MergeCells(sheet);
                }
                _groups.Clear();
            }
        }

        /// <summary>
        /// ��������������� ����� ��� ���������� �����, ������� ����� ���������� ����� ��� �����������.
        /// </summary>
        private class CellsGroup
        {
            private readonly string _groupName;
            private int _firstRow;
            private int _firstColumn;
            private int _lastRow;
            private int _lastColumn;

            public CellsGroup(string name, int fRow, int fCol, int lRow, int lCol)
            {
                _groupName = name;
                _firstRow = fRow;
                _firstColumn = fCol;
                _lastRow = lRow;
                _lastColumn = lCol;
            }

            /// <summary>
            /// ��� ������ �����.
            /// </summary>
            public string GroupName { get { return _groupName; } }

            /// <summary>
            /// ������������ ����� ������ � ������.
            /// </summary>
            /// <param name="row">������ ����� ������.</param>
            /// <param name="column">������� ����� ������.</param>
            public void RegisterCell(int row, int column)
            {
                if (row < _firstRow || column < _firstColumn)
                {
                    _firstRow = row;
                    _firstColumn = column;
                }
                if (row > _lastRow || column > _lastColumn)
                {
                    _lastRow = row;
                    _lastColumn = column;
                }
            }

            /// <summary>
            /// ���������� ������, ���� ��� ����������.
            /// </summary>
            public void MergeCells(IWorksheet sheet)
            {
                if (_lastRow > _firstRow || _lastColumn > _firstColumn)
                {
                    var range = sheet.Range[_firstRow, _firstColumn, _lastRow, _lastColumn];
                    range.Merge();
                }
            }
        }
        #endregion
    }
}
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
    /// Генератор отчетов по XML-документам заданного формата (генерируется в Excel с помощью
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

        #region Методы построения отчета

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
                throw new ArgumentException("Не заданы данные для генерации отчета (data).");
            }

            engine = new ExcelEngine {ThrowNotSavedOnDestroy = false};
            report = engine.Excel.Workbooks.Open(pathToTemplate);

            //Запоминаем имя шаблона, чтобы из него потом сделать имя файла
            templateFileName = Path.GetFileName(pathToTemplate);
            //Пытаемся получить имя файла из данных генератора данных и использовать его, если
            //снаружи имя файла отчета не задано.
            string dataGenFileName = data.DocumentElement.GetAttribute("reportName");
            if (string.IsNullOrEmpty(reportFileName) && !string.IsNullOrEmpty(dataGenFileName))
                reportFileName = dataGenFileName;


            var ranges = data.DocumentElement.SelectNodes(XPATH_RANGES);
            if (ranges == null || ranges.Count <= 0)
            {
                //Диапазонов данных нет, значит и выводить нечего.
                return;
            }
            InitTicker(ranges.Count);
            foreach (XmlNode rangeNode in ranges)
            {
                //Сначала проверим существование листа, для которого предназначен диапазон данных
                IWorksheet sheet;
                if (!SheetExists(report, rangeNode.SelectSingleNode(XPATH_SHEETNAME).Value, out sheet)) continue;

                //Лист такой есть. Теперь посмотрим, есть ли там заданный именованый диапазон
                IRange range;
                string currentRangeName = rangeNode.SelectSingleNode(XPATH_NAME).Value;
                if (!RangeExists(sheet, currentRangeName, out range)) continue;

                //Лист и диапазон найдены. Надо разложить данные. Для этого используются 2 алгоритма:
                //- простое разложение, т.е. в элементе <range> документа только один item, и надо просто
                //  разложить его значения по меткам
                //- итеративный метод, т.е. надо не только разложить значения по меткам, но и сдублировать
                //  заданный диапазон нужное кол-во раз
                XmlNodeList items;
                int itemCount = (items = rangeNode.SelectNodes(XPATH_ITEMS)) != null ? items.Count : 0;
                switch (itemCount)
                {
                    case 0:
                        //странно. в элементе <range> ничего нет. Ну на нет и суда нет. Это не повод
                        //для поднятия исключения. Если данных просто нет, то максимум, что можно сделать
                        //аккуратно, это убрать метки из диапазона, что бы они не портили внешний вид отчета
                        ClearAllMarkers(range);
                        Ticker.Ticks += 1;
                        Ticker.AddTick();
                        break;
                    case 1:
                        Ticker.Ticks += 2;
                        //Надо заполнить данными из одной строки
                        FillValuesInRange(range, rangeNode.FirstChild);
                        Ticker.AddTick();
                        //и стереть маркеры, если остались не заполненные
                        ClearAllMarkers(range);
                        Ticker.AddTick();
                        break;
                    default:
                        //Заполнить диапазон данными и продублировать его нужное кол-во раз
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
                            //Если напечатали не последний элемент, то дублируем строку со всеми настройками
                            //Но есть один момент. Копировать строку можно только если это таблица на весь лист.
                            //Если рядом с таблицей есть данные, то при добавлении строк они тоже поплывут ниже.
                            //Поэтому пока ориентируемся на старт диапазона с первой колонки. Возможно, придется
                            //в будущем сделать это поведение настраиваемым
                            if (itemNode != rangeNode.LastChild)
                            {
                                startRow += rangeSizeInRows + 1;
                                endRow += rangeSizeInRows + 1;
                                //Если при копировании диапазона снизу встречаются объединенные ячейки, то
                                //копирование не проходил (с исключением)
                                //Поэтому предварительно вставляем ниже копируемого диапазона нужное кол-во строк
                                //и убираем в них объединение
                                if (needCopy)
                                {
                                    sheet.InsertRow(startRow, currentRange.Rows.Length, ExcelInsertOptions.FormatDefault);
                                }
                                currentRange.CopyTo(sheet.Range[startRow, startCol, endRow, endCol], ExcelCopyRangeOptions.All);
                                //Докопируем в новый диапазон высоты строк, что бы не потерять сложное форматирование.
                                for (var i = 0; i < currentRange.Rows.Length; i++)
                                {
                                    sheet.Rows[startRow + i - 1].RowHeight = sheet.Rows[currentRange.Row + i - 1].RowHeight;
                                }
                            }
                            //Сначала заполняем данными. При этом для тех данных, для которых не нашлось маркеров,
                            //в отчете ничего отражено не будет
                            FillValuesInRange(currentRange, itemNode);
                            Ticker.AddTick();
                            //Но в отчете могут остаться маркеры, которые не заданы в данных. И тогда они останутся
                            //на листе, что нехорошо. Поэтому постфиксом подчищаем такие вещи.
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
                //После вывода данных какого-то конкретного диапазона необходимо сгруппировать
                //ячейки, если была задана такая необходимость в данных.
                Groups.MergeAndClear(sheet);
            }
        }

        /// <summary>
        /// Для каждого вложенного элемента value в item производит поиск маркера с именем
        /// из атрибута name и значением из контента (CDATA секция)
        /// </summary>
        /// <param name="range">Диапазон ячеек для поиска</param>
        /// <param name="item">Элемент данных, содержащий в себе список значений</param>
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
        /// Ищет маркеры в заданном диапазоне и удаляет с листа
        /// </summary>
        /// <param name="range">Диапазон ячеек листа для поиска</param>
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
        /// Проверяет, содержит ли значение ячейки маркер для замены данными
        /// </summary>
        /// <param name="cellValue">Значение ячейки</param>
        /// <param name="markers">Список маркеров, обрануженных в значении ячейки</param>
        /// <returns>true, если значение ячейки содержит маркер(ы)</returns>
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
        /// Поиск маркера в диапазоне ячеек (как правило, полученном из именованного диапазона)
        /// </summary>
        /// <param name="where">Диапазон ячеек листа для поиска</param>
        /// <param name="marker">Маркер</param>
        /// <param name="value">Значение для замены</param>
        /// <param name="typify">Признак, пытаться или нет типизировать значение <paramref name="value"/>.</param>
        /// <param name="group">Имя группы, к которой относится значение ячейки (для объединения ячеек).</param>
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

            //Странно, но SyncFusion не умеет делать поиск по контенту ячейки, пришлось делать руками (см. выше).
            //var matchedRanges = where.FindAll(marker, ExcelFindType.Text | ExcelFindType.Formula);

            if (matchedRanges == null || matchedRanges.Count == 0) return;
            foreach (var range in matchedRanges)
            {
                //Регистрируем все ячейки, что бы позже сгруппировать их.
                if (!string.IsNullOrEmpty(group))
                {
                    Groups.RegisterCell(group, range.Row, range.Column);
                }

                ReplaceMarker(range, marker, value, typify);
            }
        }

        /// <summary>
        /// Заменяет маркер в тексте ячейки значением. Старается значение задавать типизированно,
        /// как целое/дробное число, дату.
        /// </summary>
        /// <param name="where">Ячейка листа, в которой будет заменяться маркер</param>
        /// <param name="marker">Маркер</param>
        /// <param name="value">Значения для заменя</param>
        /// <param name="typify">Признак, пытаться или нет типизировать значение <paramref name="value"/>.</param>
        private static void ReplaceMarker(IRange where, string marker, object value, bool typify)
        {
            var strValue = Convert.ToString(value);

            //Если начинается с =, то это формула.
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
        /// Слишком длинные строки портят формат файла.
        /// Строки больше 994 символов будем обрезать.
        /// </summary>
        /// <param name="value">Строковое значение для обрезки</param>
        /// <returns>Исходная или обрезанная строка</returns>
        protected static string FixLongOrNullString(string value)
        {
            if (value == null) return string.Empty;
            return value.Length <= 994 ? value : value.Substring(0, 990) + "...";
        }

        /// <summary>
        /// Проверяет наличие (и возвращает) листа из книги по имени.
        /// <remarks>Если просто обратиться к листу по имени, а его не существует реально, то
        /// возникает исключение.</remarks>
        /// </summary>
        /// <param name="file">Загруженный из шаблона файл</param>
        /// <param name="sheetName">Имя листа</param>
        /// <param name="sheet">Найденный лист или null, если не найдено</param>
        /// <returns>true, если лист найден в книге</returns>
        private static bool SheetExists(IWorkbook file, string sheetName, out IWorksheet sheet)
        {
            sheet = file.Worksheets[sheetName];
            return sheet != null;
        }

        /// <summary>
        /// Проверяет наличие (и возвращает) именованного диапазона на листе.
        /// <remarks>Если просто обратиться к диапазону по имени, а его не существует, то вернеться null.
        /// Можно было бы просто обрабатывать null, но лучше подстраховаться и сделать по аналогии с 
        /// проверкой существования листа в книге.</remarks>
        /// </summary>
        /// <param name="sheet">Лист книги</param>
        /// <param name="rangeName">Имя диапазона</param>
        /// <param name="range">Найденный диапазон или null, если не найдено</param>
        /// <returns>true, если диапазон найден</returns>
        private static bool RangeExists(IWorksheet sheet, string rangeName, out IRange range)
        {
            range = sheet.Range[rangeName];
            return range != null;
        }
        #endregion

        #region Вспомогательные типы

        private readonly GroupsMgr Groups = new GroupsMgr();

        /// <summary>
        /// Вспомогательный класс для группировки ячеек.
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
        /// Вспомогательный класс для накопления ячеек, который копит координаты ячеек для объединения.
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
            /// Имя группы ячеек.
            /// </summary>
            public string GroupName { get { return _groupName; } }

            /// <summary>
            /// Регистрирует новую ячейку в группе.
            /// </summary>
            /// <param name="row">Строка новой ячейки.</param>
            /// <param name="column">Столбец новой ячейки.</param>
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
            /// Объединить ячейки, если это необходимо.
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
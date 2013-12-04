using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Xml;
using AGO.Reporting.Common;
using Common.Logging;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using HorizontalAlignment = AGO.Reporting.Common.HorizontalAlignment;
using TextDirection = AGO.Reporting.Common.TextDirection;
using VerticalAlignment = AGO.Reporting.Common.VerticalAlignment;

namespace AGO.Reporting.Service.ReportGenerators
{
    /// <summary>
    /// Генератор отчетов по XML-документам заданного формата (генерируется в Word с помощью DocIO).
    /// </summary>
    public class SyncFusionDocReportGenerator : BaseReportGenerator, IReportGenerator, IDisposable
    {
        private string templateFileName;
        private string reportFileName;

        #region IReportGeneratorResult
        public Stream Result
        {
            get
            {
                var ms = new MemoryStream();
                report.Save(ms, FormatType.Doc);
                ms.Position = 0;
                return ms;
            }
        }

        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(reportFileName))
                {
                    reportFileName = Path.ChangeExtension(templateFileName, "doc");
                }
                if (!reportFileName.EndsWith("doc"))
                {
                    reportFileName = reportFileName + ".doc";
                }
                return reportFileName;
            }
        }

    	public string ContentType
    	{
			get { return "applicaton/msword"; }
    	}

    	#endregion

        private WordDocument report;

        /// <summary>
        /// Типизированный результат работы генератора отчетов
        /// </summary>
        public WordDocument Report
        {
            get { return report; }
        }

        public void MakeReport(string pathToTemplate, XmlDocument data, CancellationToken token)
        {
        	CancellationToken = token;
            if (data == null || data.DocumentElement == null)
            {
                throw new ReportingException("Не заданы данные для генерации отчета (data).");
            }

            report = new WordDocument();
            report.Open(pathToTemplate);
            var mm = new TIFixedMailMerge(report);
            mm.RangeInTableRendered += mm_RangeInTableRendered;
            mm.CellInTableRendered += mm_CellInTableRendered;
            mm.MergeImageField += mm_MergeImageField;
            mm.MergeField += mm_MergeField;

            //Запоминаем имя шаблона, что бы из него потом сделать имя файла
            templateFileName = Path.GetFileName(pathToTemplate);
            //Пытаемся получить имя файла из данных генератора данных и использовать его, если
            //снаружи имя файла отчета не задано.
            var dataGenFileName = data.DocumentElement.GetAttribute("reportName");
            if (string.IsNullOrEmpty(reportFileName) && !string.IsNullOrEmpty(dataGenFileName))
            {
                reportFileName = dataGenFileName;
            }

            var ranges = data.DocumentElement.SelectNodes(XPATH_RANGES);
            if (ranges == null || ranges.Count <= 0)
            {
                //Диапазонов данных нет, значит и выводить нечего.
                return;
            }
            InitTicker(ranges.Count);

            foreach (XmlNode rangeNode in ranges)
            {
                var rangeEnumerator = new ReportDataRangeEnumerator(rangeNode);
                var hasSubGroups = rangeNode.SelectSingleNode("item/subRange") != null;
                if (hasSubGroups)
                {
                    mm.ExecuteNestedGroup(rangeEnumerator);
                }
                else
                {
                    mm.ExecuteGroup(rangeEnumerator);
                }
                Ticker.AddTick();
            }
            var pst = ExtractPageSetup(data);
            if (pst != null && report != null)
            {
                foreach (IWSection section in report.Sections)
                {
                    section.PageSetup.PageStartingNumber = pst.PageStartingNumber;
                    section.PageSetup.RestartPageNumbering = pst.RestartPageNumbering;
                }
            }
        }

        private static ReportFieldFormat ExtractFieldFormat(XmlElement value)
        {
            if (value == null || !value.HasAttribute("format")) return null;
            return new ReportFieldFormat(value.Attributes["format"].Value);
        }

        private static ReportCellFormat ExtractCellFormat(XmlElement value)
        {
            if (value == null || !value.HasAttribute("format")) return null;
            return new ReportCellFormat(value.Attributes["format"].Value);
        }

        private static ReportCellFormat ExtractGroupFormat(XmlElement range, string groupName)
        {
            if (range == null) return null;
            var groupFormatNode = range.SelectNodes(string.Format("group[@name='{0}']", groupName));
            if (groupFormatNode == null || groupFormatNode.Count <= 0) return null;
            if (groupFormatNode.Count > 1)
            {
                throw new ReportingException(string.Format("Для группы данных '{0}' задано более одного формата ('{1}'). Обратитесь к разработчикам.", groupName, groupFormatNode.Count));
            }
            return new GroupCellFormat(groupFormatNode[0].Attributes["format"].Value);
        }

        private static ReportPageSetup ExtractPageSetup(XmlDocument doc)
        {
            if (doc == null) return null;
            if (doc.DocumentElement == null || !doc.DocumentElement.HasAttribute("pageSetup")) return null;
            return new ReportPageSetup(doc.DocumentElement.Attributes["pageSetup"].Value);
        }

        public void Dispose()
        {
            try
            {
                TryDisposeImages();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(GetType()).Error("Ошибка при попытке очистки файлов, использованных для генерации отчета.", ex);
            }
        }

        private void TryDisposeImages()
        {
            if (_imagesToDispose != null)
            {
                foreach (var fileName in _imagesToDispose.Keys)
                {
                    _imagesToDispose[fileName].Dispose();
                    File.Delete(fileName);
                }
            }
        }

        private Dictionary<string, Image> _imagesToDispose;

        private void mm_MergeImageField(object sender, MergeImageFieldEventArgs e)
        {
            var imgFile = (string)e.FieldValue;
            if (!File.Exists(imgFile)) return;

            e.Image = Image.FromFile(imgFile);

            if (_imagesToDispose == null)
            {
                _imagesToDispose = new Dictionary<string, Image>();
            }

            _imagesToDispose.Add(imgFile, e.Image);
        }

        private static void mm_MergeField(object sender, MergeFieldEventArgs e)
        {
            var fre = e as FieldRenderedEventArgs;
            if (fre != null)
            {
                var fmt = ExtractFieldFormat(fre.ValueNode);
                if (fmt != null)
                {
                    if (fre.CharacterFormat != null)
                    {
                        fre.CharacterFormat.Bold = fmt.Bold;
                    }
                    if (fre.CurrentMergeField.OwnerParagraph != null)
                    {
                        var pfmt = fre.CurrentMergeField.OwnerParagraph.ParagraphFormat;
                        pfmt.HorizontalAlignment = ToSyncFusion(fmt.HorizontalAlignment);
                        pfmt.PageBreakBefore = fmt.PageBreakBefore;
                        pfmt.PageBreakAfter = fmt.PageBreakAfter;
                    }
                }
            }
        }

        private void mm_CellInTableRendered(object sender, CellInTableRenderedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.GroupName))
            {
                Groups.RegisterCell(e.GroupName, e.Table, e.Row, e.Column);
            }
            if (e.Value != null)
            {
                ApplyFormatting(e.Table.Rows[e.Row].Cells[e.Column], ExtractCellFormat(e.Value));
            }
        }

        private void mm_RangeInTableRendered(object sender, RangeInTableRenderedEventArgs e)
        {
            Groups.MergeAndClear(e.Range);
        }

        private static void ApplyFormatting(WTableCell cell, ReportCellFormat fmt)
        {
            if (cell == null || fmt == null) return;

            cell.CellFormat.VerticalAlignment = ToSyncFusion(fmt.VerticalAlignment);
            if (cell.LastParagraph != null)
            {
                cell.LastParagraph.ParagraphFormat.HorizontalAlignment = ToSyncFusion(fmt.HorizontalAlignment);
                foreach (var pi in cell.LastParagraph.ChildEntities)
                {
                    if (pi is WTextRange)
                    {
                        ((WTextRange)pi).CharacterFormat.Bold = fmt.Bold;
                        if (fmt.FontSize > 0)
                            ((WTextRange)pi).CharacterFormat.FontSize = fmt.FontSize;
                    }
                }
            }
            cell.CellFormat.TextDirection = ToSyncFusion(fmt.TextDirection);
        }

        private static Syncfusion.DocIO.DLS.VerticalAlignment ToSyncFusion(VerticalAlignment tinAlignment)
        {
            switch (tinAlignment)
            {
                case VerticalAlignment.Top: return Syncfusion.DocIO.DLS.VerticalAlignment.Top;
                case VerticalAlignment.Middle: return Syncfusion.DocIO.DLS.VerticalAlignment.Middle;
                case VerticalAlignment.Bottom: return Syncfusion.DocIO.DLS.VerticalAlignment.Bottom;
                default: return Syncfusion.DocIO.DLS.VerticalAlignment.Top;
            }
        }

        private static Syncfusion.DocIO.DLS.HorizontalAlignment ToSyncFusion(HorizontalAlignment tinAlignment)
        {
            switch (tinAlignment)
            {
                case HorizontalAlignment.Left: return Syncfusion.DocIO.DLS.HorizontalAlignment.Left;
                case HorizontalAlignment.Center: return Syncfusion.DocIO.DLS.HorizontalAlignment.Center;
                case HorizontalAlignment.Right: return Syncfusion.DocIO.DLS.HorizontalAlignment.Right;
                default: return Syncfusion.DocIO.DLS.HorizontalAlignment.Left;
            }
        }

        private static Syncfusion.DocIO.DLS.TextDirection ToSyncFusion(TextDirection tinDirection)
        {
            switch (tinDirection)
            {
                case TextDirection.Horizontal: return Syncfusion.DocIO.DLS.TextDirection.Horizontal;
                case TextDirection.VerticalBottomToTop: return Syncfusion.DocIO.DLS.TextDirection.VerticalBottomToTop;
                case TextDirection.VerticalTopToBottom: return Syncfusion.DocIO.DLS.TextDirection.VerticalTopToBottom;
                default: return Syncfusion.DocIO.DLS.TextDirection.Horizontal;
            }
        }

        #region Вспомогательные типы

        private readonly GroupsMgr Groups = new GroupsMgr();

        /// <summary>
        /// Вспомогательный класс для группировки ячеек.
        /// </summary>
        private class GroupsMgr
        {
            private readonly Dictionary<string, CellsGroup> _groups = new Dictionary<string, CellsGroup>();

            public void RegisterCell(string group, WTable table, int row, int column)
            {
                if (!_groups.ContainsKey(group))
                {
                    var cg = new CellsGroup(table, group, row, column, row, column);
                    _groups.Add(cg.GroupName, cg);
                }
                else
                {
                    var gr = _groups[group];
                    gr.RegisterCell(row, column);
                }
            }

            public void MergeAndClear(XmlElement range)
            {
                foreach (var group in _groups.Keys)
                {
                    _groups[group].MergeCells(range);
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
            private readonly WTable _targetTable;
            private int _firstRow;
            private int _firstColumn;
            private int _lastRow;
            private int _lastColumn;

            public CellsGroup(WTable table, string name, int fRow, int fCol, int lRow, int lCol)
            {
                _targetTable = table;
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
            public void MergeCells(XmlElement range)
            {
                if (_targetTable == null)
                {
                    throw new ReportingException(string.Format("Не задана таблица. Группа '{0}'", _groupName));
                }

                if (_lastColumn > _firstColumn)
                {
                    for (int row = _firstRow; row <= _lastRow; row++)
                    {
                        //Перед объединение ячеек очищаем данные из всех объединяемых, кроме первой,
                        //т.к. был замечен баг, что в ворде потом пытаешся изменить размер столбца 
                        //перетаскиванием, и все объединение сразу слетает, а в разгруппировавшихся
                        //ячейках появляются скрытые до этого одинаковые данные.
                        for (int c = _firstColumn + 1; c <= _lastColumn; c++)
                        {
                            WTableCell cell = _targetTable.Rows[row].Cells[c];
                            cell.ChildEntities.Clear();
                        }

                        _targetTable.ApplyHorizontalMerge(row, _firstColumn, _lastColumn);
                    }
                }
                if (_lastRow > _firstRow)
                {
                    for (int column = _firstColumn; column <= _lastColumn; column++)
                    {
                        //Перед объединение ячеек очищаем данные из всех объединяемых, кроме первой,
                        //т.к. был замечен баг, что в ворде потом пытаешся изменить размер столбца 
                        //перетаскиванием, и все объединение сразу слетает, а в разгруппировавшихся
                        //ячейках появляются скрытые до этого одинаковые данные.
                        for (int r = _firstRow + 1; r <= _lastRow; r++)
                        {
                            WTableCell cell = _targetTable.Rows[r].Cells[column];
                            cell.ChildEntities.Clear();
                        }

                        _targetTable.ApplyVerticalMerge(column, _firstRow, _lastRow);
                    }
                }

                //старый вариант форматирования - не удаляем, т.к. везде сразу отвалятся итоги в таблицах.
                if (_firstRow == _lastRow && _lastColumn > _firstColumn && _firstColumn == 0)
                {
                    //Объединяются несколько ячеек по горизонтали, начиная с первого столбца.
                    //Считаем, что это строка для Итого и выравниваем в ней текст вправо.
                    var firstCell = _targetTable.Rows[_firstRow].Cells[_firstColumn];
                    firstCell.CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Top;
                    if (firstCell.LastParagraph != null)
                    {
                        firstCell.LastParagraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Right;
                        firstCell.CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Middle;
                    }
                }

                ApplyFormatting(_targetTable.Rows[_firstRow].Cells[_firstColumn], ExtractGroupFormat(range, _groupName));
            }
        }
        #endregion
    }
}
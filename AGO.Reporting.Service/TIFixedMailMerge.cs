using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;

// ReSharper disable UnassignedField.Local
namespace Syncfusion.DocIO.DLS
{
    public static class AccessabilityPatching
    {
        public static bool ConvertedToText(this WMergeField field)
        {
            return
                (bool)
                typeof(WField).GetField("m_bConvertedToText", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(
                    field);
        }

        public static bool ConvertedToText(this WField field)
        {
            return
                (bool)
                typeof(WField).GetField("m_bConvertedToText", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(
                    field);
        }

        public static void SetConvertedToText(this WField field, bool value)
        {
            typeof(WField).GetField("m_bConvertedToText", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(
                field, value);
        }

        public static bool GetIsMailMerge(this WordDocument doc)
        {
            return
                (bool)
                typeof(WordDocument).GetField("IsMailMerge", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(
                    doc);
        }

        public static void SetIsMailMerge(this WordDocument doc, bool value)
        {
            typeof(WordDocument).GetField("IsMailMerge", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(doc,
                                                                                                                   value);
        }

        public static void CloneTo(this WSectionCollection coll, EntityCollection destColl)
        {
            int num = 0;
            int count = coll.Count;
            while (num < count)
            {
                destColl.Add(coll[num].Clone());
                num++;
            }
        }

        public static int ShiftStartToEnd(this TextBodySelection tbs, int endIndexShift, int pEndIndexShift)
        {
            return (int)typeof(TextBodySelection).InvokeMember(
                "ShiftStartToEnd", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, 
                null, tbs, new object[] { endIndexShift, pEndIndexShift });
        }

        public static bool GetRemoveEmpty(this WParagraph p)
        {
            return
                (bool)
                typeof(WParagraph).GetProperty("RemoveEmpty", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(
                    p, null);
        }

        public static void SetRemoveEmpty(this WParagraph p, bool value)
        {
            typeof(WParagraph).GetProperty("RemoveEmpty", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(p,
                                                                                                                    value,
                                                                                                                    null);
        }

        public static ArrayList GetMergeFields(this WIfField f)
        {
            return
                (ArrayList)
                typeof(WIfField).GetProperty("MergeFields", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(
                    f, null);
        }
    }
}

namespace AGO.Reporting.Service
{
    internal class DataReaderEnumerator : IRowsEnumerator
    {
        // Fields
        private string[] m_columnNames;
        private int m_currRowIndex = -1;
        private IDataReader m_dataReader;
        private ArrayList m_rows;

        // Methods
        public DataReaderEnumerator(IDataReader dataReader)
        {
            this.m_dataReader = dataReader;
            this.m_rows = new ArrayList();
            this.m_columnNames = new string[this.m_dataReader.FieldCount];
            for (int i = 0; i < this.m_dataReader.FieldCount; i++)
            {
                this.m_columnNames[i] = this.m_dataReader.GetName(i);
            }
            while (this.m_dataReader.Read())
            {
                ArrayList list = new ArrayList();
                for (int j = 0; j < this.m_dataReader.FieldCount; j++)
                {
                    list.Add(this.m_dataReader[j]);
                }
                this.m_rows.Add(list);
            }
        }

        public object GetCellValue(string columnName)
        {
            ArrayList currentRow = this.CurrentRow;
            if (currentRow == null)
            {
                return null;
            }
            return currentRow[this.m_dataReader.GetOrdinal(columnName)];
        }

        public bool NextRow()
        {
            if (this.m_currRowIndex < this.RowsCount)
            {
                this.m_currRowIndex++;
            }
            return !this.IsEnd;
        }

        public void Reset()
        {
            this.m_currRowIndex = -1;
        }

        // Properties
        public string[] ColumnNames
        {
            get { return this.m_columnNames; }
        }

        protected ArrayList CurrentRow
        {
            get
            {
                if (this.m_currRowIndex >= this.RowsCount)
                {
                    return null;
                }
                if (this.m_rows == null)
                {
                    return null;
                }
                return (this.m_rows[this.m_currRowIndex] as ArrayList);
            }
        }

        public int CurrentRowIndex
        {
            get { return this.m_currRowIndex; }
        }

        public bool IsEnd
        {
            get { return (this.m_currRowIndex >= this.RowsCount); }
        }

        public bool IsLast
        {
            get { return (this.m_currRowIndex >= (this.RowsCount - 1)); }
        }

        public int RowsCount
        {
            get { return this.m_rows.Count; }
        }

        public string TableName
        {
            get { return this.m_dataReader.GetSchemaTable().TableName; }
        }
    }

    internal class DataTableEnumerator : IRowsEnumerator
    {
        // Fields
        private string[] m_columnsNames;
        private int m_currRowIndex;
        private DataRow m_row;
        private DataTable m_table;

        // Methods
        public DataTableEnumerator(DataRow row)
        {
            this.m_currRowIndex = -1;
            this.m_row = row;
            this.ReadColumnNames(row.Table);
        }

        public DataTableEnumerator(DataTable table)
        {
            this.m_currRowIndex = -1;
            this.m_table = table;
            this.ReadColumnNames(this.m_table);
        }

        public object GetCellValue(string columnName)
        {
            DataRow currentRow = this.CurrentRow;
            if (currentRow == null)
            {
                return null;
            }
            return currentRow[columnName];
        }

        public bool NextRow()
        {
            if (this.m_currRowIndex < this.RowsCount)
            {
                this.m_currRowIndex++;
            }
            return !this.IsEnd;
        }

        private void ReadColumnNames(DataTable table)
        {
            this.m_columnsNames = new string[table.Columns.Count];
            for (int i = 0; i < this.m_columnsNames.Length; i++)
            {
                this.m_columnsNames[i] = table.Columns[i].ColumnName;
            }
        }

        public void Reset()
        {
            this.m_currRowIndex = -1;
        }

        // Properties
        public string[] ColumnNames
        {
            get { return this.m_columnsNames; }
        }

        protected DataRow CurrentRow
        {
            get
            {
                if (this.m_currRowIndex >= this.RowsCount)
                {
                    return null;
                }
                if (this.m_table == null)
                {
                    return this.m_row;
                }
                return this.m_table.Rows[this.m_currRowIndex];
            }
        }

        public int CurrentRowIndex
        {
            get { return this.m_currRowIndex; }
        }

        public bool IsEnd
        {
            get { return (this.m_currRowIndex >= this.RowsCount); }
        }

        public bool IsLast
        {
            get { return (this.m_currRowIndex >= (this.RowsCount - 1)); }
        }

        public int RowsCount
        {
            get
            {
                if (this.m_table == null)
                {
                    return 1;
                }
                return this.m_table.Rows.Count;
            }
        }

        public string TableName
        {
            get
            {
                if (this.m_table == null)
                {
                    return "";
                }
                return this.m_table.TableName;
            }
        }
    }

    internal class DataViewEnumerator : IRowsEnumerator
    {
        // Fields
        private string[] m_columnNames;
        private int m_currRowIndex = -1;
        private DataView m_dataView;
        private DataRow m_row;
        private DataTable m_table;

        // Methods
        public DataViewEnumerator(DataView dataView)
        {
            this.m_dataView = dataView;
            this.m_table = dataView.Table;
            this.ReadColumnNames(this.m_table);
        }

        public object GetCellValue(string columnName)
        {
            DataRow currentRow = this.CurrentRow;
            if (currentRow == null)
            {
                return null;
            }
            return currentRow[columnName];
        }

        public bool NextRow()
        {
            if (this.m_currRowIndex < this.RowsCount)
            {
                this.m_currRowIndex++;
            }
            return !this.IsEnd;
        }

        private void ReadColumnNames(DataTable dataTable)
        {
            this.m_columnNames = new string[dataTable.Columns.Count];
            for (int i = 0; i < this.m_columnNames.Length; i++)
            {
                this.m_columnNames[i] = this.m_table.Columns[i].ColumnName;
            }
        }

        public void Reset()
        {
            this.m_currRowIndex = -1;
        }

        // Properties
        public string[] ColumnNames
        {
            get { return this.m_columnNames; }
        }

        protected DataRow CurrentRow
        {
            get
            {
                if (this.m_currRowIndex >= this.RowsCount)
                {
                    return null;
                }
                if (this.m_dataView == null)
                {
                    return this.m_row;
                }
                return this.m_dataView[this.m_currRowIndex].Row;
            }
        }

        public int CurrentRowIndex
        {
            get { return this.m_currRowIndex; }
        }

        public bool IsEnd
        {
            get { return (this.m_currRowIndex >= this.RowsCount); }
        }

        public bool IsLast
        {
            get { return (this.m_currRowIndex >= (this.RowsCount - 1)); }
        }

        public int RowsCount
        {
            get
            {
                if (this.m_dataView == null)
                {
                    return 1;
                }
                return this.m_dataView.Count;
            }
        }

        public string TableName
        {
            get
            {
                if (this.m_dataView == null)
                {
                    return "";
                }
                return this.m_dataView.Table.TableName;
            }
        }
    }

    internal class PseudoMergeField
    {
        // Fields
        private bool m_fitMailMerge;
        private string m_name;
        private Regex m_nameExp;
        private string m_value;

        // Methods
        internal PseudoMergeField(string fieldText)
        {
            if (fieldText != null)
            {
                if (fieldText.IndexOf("MERGEFIELD") == -1)
                {
                    char[] separator = new char[] { '"' };
                    string[] strArray = fieldText.Split(separator);
                    if (strArray.Length == 1)
                    {
                        this.m_value = fieldText.Trim();
                    }
                    else if (strArray.Length == 3)
                    {
                        this.m_value = strArray[1];
                    }
                    else
                    {
                        this.m_value = string.Empty;
                    }
                }
                else
                {
                    Match match = this.NameExpression.Match(fieldText);
                    if (match.Groups.Count > 1)
                    {
                        this.m_name = match.Groups[1].Value;
                        this.m_fitMailMerge = true;
                    }
                }
            }
        }

        // Properties
        internal bool FitMailMerge
        {
            get { return this.m_fitMailMerge; }
        }

        internal string Name
        {
            get { return this.m_name; }
        }

        private Regex NameExpression
        {
            get
            {
                if (this.m_nameExp == null)
                {
                    this.m_nameExp = new Regex("MERGEFIELD\\s+\"?([^\"]+)\"");
                }
                return this.m_nameExp;
            }
        }

        internal string Value
        {
            get { return this.m_value; }
            set { this.m_value = value; }
        }
    }

    /// <summary>
    /// Енумератор для диапазона данных нашей системы отчетности.
    /// </summary>
    public class ReportDataRangeEnumerator : IRowsEnumerator
    {
        private const string XPATH_VALUE_FMT = "value[@name = '{0}']";
        private const string XPATH_ALL_VALUE_NAMES = "item/value/@name";
        private const string XPATH_SUBRANGE_FMT = "subRange[@name = '{0}']";

        private string[] _columnNames;
        private int _currentItemIndex;
        private readonly string _rangeName;
        private readonly XmlElement _rangeNode;
        private readonly XmlNodeList _items;

        public ReportDataRangeEnumerator(XmlNode range)
        {
            _currentItemIndex = -1;
            _rangeNode = (XmlElement) range;
            _rangeName = _rangeNode.Attributes["name"].Value;
            _items = range.SelectNodes("item");
            ReadColumnNames();
        }

        private void ReadColumnNames()
        {
            var values = _rangeNode.SelectNodes(XPATH_ALL_VALUE_NAMES);
            if (values == null)
            {
                _columnNames = new string[0];
                return;
            }

            var list = new List<string>();
            foreach (XmlNode valueName in values)
            {
                if (!list.Contains(valueName.Value))
                {
                    list.Add(valueName.Value);
                }
            }
            _columnNames = list.ToArray();
        }

        public void Reset()
        {
            _currentItemIndex = -1;
        }

        public bool NextRow()
        {
            if (_currentItemIndex < _items.Count)
            {
                _currentItemIndex++;
            }
            return !IsEnd;
        }

        internal XmlNode GetValueNode(string valueName)
        {
            return _items[_currentItemIndex].SelectSingleNode(string.Format(XPATH_VALUE_FMT, valueName));
        }

        /// <summary>
        /// Возвращает вложенный в текущий item диапазон по его имени.
        /// </summary>
        /// <param name="name">Имя вложенного диапазона.</param>
        /// <returns>Енумератор вложенного диапазона данных</returns>
        internal ReportDataRangeEnumerator GetSubRange(string name)
        {
            var currentItem = _items[_currentItemIndex];
            var subRangeNode = currentItem.SelectSingleNode(string.Format(XPATH_SUBRANGE_FMT, name));

            return subRangeNode != null ? new ReportDataRangeEnumerator(subRangeNode) : null;
        }

        public string GetCellGroup(string columnName)
        {
            if (IsEnd) return null;

            var valueNode = GetValueNode(columnName);
            if (valueNode == null) return null;
            XmlNode groupNode;
            return (groupNode = valueNode.SelectSingleNode("@group")) != null ? groupNode.Value : null;
        }

        public object GetCellValue(string columnName)
        {
            if (IsEnd) return null;

            var valueNode = GetValueNode(columnName);
            return valueNode == null ? null : valueNode.InnerText;
        }

//        public bool HasColumn(int index)
//        {
//            return index >= 0 && index < _columnNames.Length;
//        }

        public string[] ColumnNames
        {
            get { return _columnNames; }
        }

        public int RowsCount
        {
            get { return _items.Count; }
        }

        public int CurrentRowIndex
        {
            get { return _currentItemIndex; }
        }

        public string TableName
        {
            get { return _rangeName; }
        }

        internal XmlElement RangeNode
        {
            get { return _rangeNode; }
        }

        public bool IsEnd
        {
            get { return _currentItemIndex >= RowsCount; }
        }

        public bool IsLast
        {
            get { return _currentItemIndex >= (RowsCount - 1); }
        }
    }

    /// <summary>
    /// Агрументы события окончания рендеринга диапазона данных в <see cref="WTable">таблицу</see>.
    /// </summary>
    public sealed class RangeInTableRenderedEventArgs : EventArgs
    {
        private readonly string _range;
        private readonly XmlElement _rangeNode;

        public RangeInTableRenderedEventArgs(string range, XmlElement node)
        {
            _range = range;
            _rangeNode = node;
        }

        public string RangeName { get { return _range; } }

        public XmlElement Range { get { return _rangeNode; } }
    }

    /// <summary>
    /// Аргументы события окончания рендеринга <see cref="WTableCell">ячейки</see> <see cref="WTable">таблицы</see>.
    /// </summary>
    public sealed class CellInTableRenderedEventArgs : EventArgs
    {
        private readonly WTable _table;
        private readonly int _row;
        private readonly int _column;
        private readonly string _group;
        private readonly XmlElement _value;

        public CellInTableRenderedEventArgs(WTable t, int r, int c, string g, XmlElement value)
        {
            _table = t;
            _row = r;
            _column = c;
            _group = g;
            _value = value;
        }

        public WTable Table { get { return _table; } }

        public int Row { get { return _row; } }

        public int Column { get { return _column; } }

        public string GroupName { get { return _group; } }

        public XmlElement Value { get { return _value; } }
    }

    /// <summary>
    /// Аргументы
    /// </summary>
    public sealed class FieldRenderedEventArgs: MergeFieldEventArgs
    {
        public FieldRenderedEventArgs(IWordDocument doc, string tableName, int rowIndex, IWMergeField field, object value, XmlElement valueNode) : base(doc, tableName, rowIndex, field, value)
        {
            this.valueNode = valueNode;
        }

        private readonly XmlElement valueNode;

        public XmlElement ValueNode { get { return valueNode; } }
    }

    public class TIFixedMailMerge
    {
        // Fields
        private bool m_bBeginGroupFound;
        private bool m_bClearFields = true;
        private bool m_bEndGroupFound;
        private bool m_bIsNested;
        private bool m_bRemoveEmptyPara;
        //removed by artem1 due to implement our nested groups (xml, not sql data tables).
        //private ArrayList m_commands;
        //removed by artem1 due to implement our nested groups (xml, not sql data tables).
        //private DbConnection m_conn;
        private WSectionCollection m_contentSections;
        //removed by artem1 due to implement our nested groups (xml, not sql data tables).
        //private DataSet m_curDataSet;
        private WordDocument m_doc;
        private GroupSelector m_groupSelector;
        private Stack m_groupSelectors;
        //removed by artem1 due to implement our nested groups (xml, not sql data tables).
        //private bool m_isSqlConnection;
        private Hashtable m_mappedFields;
        private string[] m_names;
        private HybridDictionary m_nestedEnums;
        private string[] m_values;
        //removed by artem1 due to implement our nested groups (xml, not sql data tables).
        //private Regex m_varCmdRegex;

        private IRowsEnumerator topEnumerator;

        // Events
        public event MergeFieldEventHandler MergeField;

        public event MergeImageFieldEventHandler MergeImageField;

        // Methods
        public TIFixedMailMerge(WordDocument document)
        {
            this.m_bClearFields = true;
            this.m_doc = document;
            //this.m_contentSections = new WSectionCollection(); fixed internal constructor. same logic. artem1
            this.m_contentSections = new WSectionCollection(null);
            this.m_groupSelector = new GroupSelector(new GroupSelector.GroupFound(this.OnGroupFound));
        }

        private bool CheckSelection(IRowsEnumerator rowsEnum)
        {
            if (rowsEnum.RowsCount > 0)
            {
                return true;
            }
            if (!this.m_bClearFields && !this.m_bRemoveEmptyPara)
            {
                return true;
            }
            GroupSelector groupSelector = this.m_groupSelector;
            if (groupSelector.GroupSelection != null)
            {
                //this.HideFields(groupSelector.GroupSelection.TextBody.Items); fixed internal prop. artem1
                this.HideFields((BodyItemCollection)groupSelector.GroupSelection.TextBody.ChildEntities);
                if (this.m_bRemoveEmptyPara)
                {
                    //this.RemoveEmptyPara(groupSelector.GroupSelection.TextBody.Items); fixed internal prop. artem1
                    this.RemoveEmptyPara((BodyItemCollection)groupSelector.GroupSelection.TextBody.ChildEntities);
                }
            }
            else if (groupSelector.RowSelection != null)
            {
                int startRowIndex = groupSelector.RowSelection.StartRowIndex;
                int endRowIndex = groupSelector.RowSelection.EndRowIndex;
                for (int i = startRowIndex; i <= endRowIndex; i++)
                {
                    this.HideFields(groupSelector.RowSelection.Table.Rows[i]);
                    if (this.m_bRemoveEmptyPara)
                    {
                        this.RemoveEmptyPara(groupSelector.RowSelection.Table.Rows[i]);
                    }
                }
            }
            return false;
        }

        private void ExecuteForParagraphs(BodyItemCollection paragraphs, IRowsEnumerator rowsEnum)
        {
            ITextBodyItem item = null;
            int num = 0;
            int count = paragraphs.Count;
            while (num < count)
            {
                item = (ITextBodyItem)paragraphs[num];
                if (item is IWParagraph)
                {
                    WParagraph paragraph = item as WParagraph;
                    int num3 = 0;
                    int num4 = paragraph.Items.Count;
                    while (num3 < num4)
                    {
                        WMergeField field = paragraph[num3] as WMergeField;
                        if (field != null)
                        {
                            if (field.Prefix.StartsWith("Image"))
                            {
                                this.UpdateImageFieldValue(field, paragraph, rowsEnum);
                            }
                            else
                            {
                                this.UpdateFieldValue(field, rowsEnum);
                            }
                        }
                        else if (paragraph[num3] is WField)
                        {
                            WField field2 = paragraph[num3] as WField;
                            if (field2.FieldType == FieldType.FieldNext)
                            {
                                if ((rowsEnum != null) && !rowsEnum.IsEnd)
                                {
                                    rowsEnum.NextRow();
                                }
                                this.HideField(field2);
                            }
                            else if (field2.FieldType == FieldType.FieldIf)
                            {
                                this.UpdateIfFieldValue(field2 as WIfField, rowsEnum);
                            }
                        }
                        else if (paragraph[num3] is WTextBox)
                        {
                            WTextBox box = paragraph[num3] as WTextBox;
                            this.ExecuteForParagraphs((BodyItemCollection)box.TextBoxBody.ChildEntities, rowsEnum);
                        }
                        num3++;
                    }
                    if (this.m_bRemoveEmptyPara)
                    {
                        this.RemoveEmptyPara(paragraph);
                    }
                }
                else if (item is IWTable)
                {
                    IWTable table = item as IWTable;
                    if (table != null)
                    {
                        this.ExecuteForTable(table, rowsEnum);
                    }
                }
                num++;
            }
        }

        private void ExecuteForSection(IWSection sec, IRowsEnumerator rowsEnum)
        {
            //this.ExecuteForParagraphs(sec.Body.Items, rowsEnum); fixed internal prop. same as value. artem1
            this.ExecuteForParagraphs((BodyItemCollection)sec.Body.ChildEntities, rowsEnum);
            for (int i = 0; i < 6; i++)
            {
                BodyItemCollection childEntities = (BodyItemCollection)sec.HeadersFooters[i].ChildEntities;
                if (childEntities.Count > 0)
                {
                    this.ExecuteForParagraphs(childEntities, rowsEnum);
                }
            }
        }

        private void ExecuteForTable(IWTable table, IRowsEnumerator rowsEnum)
        {
            WTableRow row = null;
            WTableCell cell = null;
            int num = 0;
            int count = table.Rows.Count;
            while (num < count)
            {
                row = table.Rows[num];
                int num3 = 0;
                int num4 = row.Cells.Count;
                while (num3 < num4)
                {
                    cell = row.Cells[num3];
                    this.ExecuteForParagraphs((BodyItemCollection)cell.ChildEntities, rowsEnum);
                    num3++;
                }
                num++;
            }
        }

        public void ExecuteGroup(IRowsEnumerator rowsEnum)
        {
            this.Document.SetIsMailMerge(true);
            WSection section = null;
            int num = 0;
            int count = this.Document.Sections.Count;
            while (num < count)
            {
                section = this.Document.Sections[num];
                this.ExecuteGroup(section, rowsEnum);
                num++;
            }
            this.Document.SetIsMailMerge(false);
        }

        //added by artem1 for nesting our xml-based groups
        public void ExecuteNestedGroup(IRowsEnumerator rowsEnum)
        {
            m_bIsNested = true;
            topEnumerator = rowsEnum;
            ExecuteGroup(rowsEnum);
            if (m_nestedEnums != null)
            {
                m_nestedEnums.Clear();
                m_nestedEnums = null;
            }
            topEnumerator = null;
            m_bIsNested = false;
        }

        private void ExecuteGroup(WSection section, IRowsEnumerator rowsEnum)
        {
            this.m_groupSelector.ProcessGroups(section.Body, rowsEnum);
            for (int i = 0; i < 6; i++)
            {
                WTextBody body = section.HeadersFooters[i];
                //if (body.Items.Count > 0) fixed internal prop. same as value. artem1
                if (body.ChildEntities.Count > 0)
                {
                    this.m_groupSelector.ProcessGroups(body, rowsEnum);
                }
            }
        }

        public event EventHandler<CellInTableRenderedEventArgs> CellInTableRendered;

        private int ExecuteGroupForRowSelection(WTable table, int startRowIndex, int count, IRowsEnumerator rowsEnum)
        {
            int num = table.Rows.Count;
            int endRow = (startRowIndex + count) - 1;
            WTableRow row = null;
            WTableCell textBody = null;
            for (int i = startRowIndex; i <= endRow; i++)
            {
                row = table.Rows[i];
                int cellIndex = 0;
                int num5 = row.Cells.Count;
                while (cellIndex < num5)
                {
                    textBody = row.Cells[cellIndex];
                    this.ExecuteGroupForSelection(textBody, 0, -1, 0, -1, rowsEnum);
                    cellIndex++;
                }
            }
            int num6 = 0;
            if (this.m_bIsNested)
            {
                string tableName = this.FindNestedGroup(startRowIndex, endRow, table);
                int num7 = table.Rows.Count;
                while (tableName != null)
                {
                    num6 = table.Rows.Count - num7;
                    endRow += num6;
                    startRowIndex += num6;
                    num7 = table.Rows.Count;
                    IRowsEnumerator enumerator = this.GetEnum(tableName);
                    if (enumerator != null)
                    {
                        this.GroupSelectors.Push(this.m_groupSelector);
                        this.m_groupSelector = new GroupSelector(new GroupSelector.GroupFound(this.OnGroupFound));
                        this.m_groupSelector.ProcessGroups(table, startRowIndex, endRow, enumerator);
                        //removed by artem1 due to implement our nested groups (xml, not sql data tables).
                        //this.CurrentDataSet.Tables.Remove(tableName);
                        this.m_groupSelector = this.GroupSelectors.Pop() as GroupSelector;
                    }
                    string str2 = tableName;
                    tableName = this.FindNestedGroup(startRowIndex, endRow, table);
                    if (str2 == tableName)
                    {
                        break;
                    }
                }
            }
            num6 = table.Rows.Count - num;
            return (count + num6);
        }

        private void ExecuteGroupForSelection(WTextBody textBody,
            int itemStart, int itemEnd, int pItemStart, int pItemEnd, IRowsEnumerator rowsEnum)
        {
            if (itemEnd < 0)
            {
                //itemEnd = textBody.Items.Count - 1; fixed internal prop. same as value. artem1
                itemEnd = textBody.ChildEntities.Count - 1;
            }
            int num = itemStart;
            int num2 = itemEnd;
            while (num <= num2)
            {
                WTable table;
                int num3;
                int num4;
                //TextBodyItem item = textBody.Items[num]; fixed internal prop. same as value. artem1
                TextBodyItem item = (TextBodyItem)((BodyItemCollection)textBody.ChildEntities)[num];
                switch (item.EntityType)
                {
                    case EntityType.Paragraph:
                        {
                            WParagraph paragraph = item as WParagraph;
                            int num5 = (num == 0) ? pItemStart : 0;
                            int num6 = ((num == num2) && (pItemEnd > -1)) ? pItemEnd : (paragraph.Items.Count - 1);
                            for (int i = num5; i <= num6; i++)
                            {
                                WField field = paragraph.Items[i] as WField;
                                if (field != null)
                                {
                                    if (field is WMergeField)
                                    {
                                        WMergeField field2 = field as WMergeField;
                                        if (!IsBeginGroup(field2) && !IsEndGroup(field2))
                                        {
                                            if (field2.Prefix.StartsWith("Image"))
                                            {
                                                this.UpdateImageFieldValue(field2, paragraph, rowsEnum);
                                            }
                                            else
                                            {
                                                this.UpdateFieldValue(field2, rowsEnum);
                                            }

                                            //added by artem1. Это оказалось наилучшим местом для вызова события,
                                            //с учетом возможности нескольких полей в одной ячейке и нескольких таблиц
                                            //в одном диапазоне.
                                            if (textBody is WTableCell)
                                            {
                                                var cell = (WTableCell) textBody;
                                                if (CellInTableRendered != null)
                                                {
                                                    var ourEnumerator = (rowsEnum as ReportDataRangeEnumerator);
                                                    string group = null;
                                                    XmlElement value = null;
                                                    if (ourEnumerator != null)
                                                    {
                                                        group = ourEnumerator.GetCellGroup(field2.FieldName);
                                                        value = (XmlElement) ourEnumerator.GetValueNode(field2.FieldName);
                                                    }

                                                    CellInTableRendered(this, new CellInTableRenderedEventArgs(cell.OwnerRow.Owner as WTable, cell.OwnerRow.GetRowIndex(), cell.GetCellIndex(), group, value));
                                                }
                                            }
                                            //end added by artem1

                                            goto Label_027A;
                                        }
                                        if ((!this.m_bIsNested || !IsBeginGroup(field2)) ||
                                            ((this.NestedEnums[field2.FieldName] != null) ||
                                             (field2.Prefix == "TableStart")))
                                        {
                                            goto Label_027A;
                                        }
                                        string fieldName = field2.FieldName;
                                        if (fieldName == string.Empty)
                                        {
                                            goto Label_027A;
                                        }
                                        IRowsEnumerator enumerator = this.GetEnum(fieldName);
                                        if (enumerator == null)
                                        {
                                            goto Label_027A;
                                        }
                                        //int count = textBody.Items.Count; fixed internal prop. same as value. artem1
                                        int count = textBody.ChildEntities.Count;
                                        this.GroupSelectors.Push(this.m_groupSelector);
                                        this.m_groupSelector =
                                            new GroupSelector(new GroupSelector.GroupFound(this.OnGroupFound));
                                        this.m_groupSelector.ProcessGroups(textBody, enumerator);
                                        int selectedBodyItemsCount = this.m_groupSelector.SelectedBodyItemsCount;
                                        if (selectedBodyItemsCount == -1)
                                        {
                                            throw new ApplicationException("Group \"" + fieldName +
                                                                           "\" is missing in the source document.");
                                        }
                                        if (selectedBodyItemsCount > 0)
                                        {
                                            //int num10 = textBody.Items.Count - count; fixed internal prop. same as value. artem1
                                            int num10 = textBody.ChildEntities.Count - count;
                                            num += (num10 + selectedBodyItemsCount) - 1;
                                            num2 += num10;
                                            itemEnd = num2;
                                        }
                                        else
                                        {
                                            this.HideField(field2);
                                        }
                                        //removed by artem1 due to implement our nested groups (xml, not sql data tables).
                                        //this.CurrentDataSet.Tables.Remove(fieldName);
                                        this.m_groupSelector = this.GroupSelectors.Pop() as GroupSelector;
                                        break;
                                    }
                                    if (field is WIfField)
                                    {
                                        this.UpdateIfFieldValue(field as WIfField, rowsEnum);
                                    }
                                Label_027A:
                                    ;
                                }
                            }
                            if (this.m_bRemoveEmptyPara)
                            {
                                this.RemoveEmptyPara(paragraph);
                            }
                            goto Label_0299;
                        }
                    case EntityType.Table:
                        table = item as WTable;
                        num3 = 0;
                        num4 = table.Rows.Count;
                        goto Label_0073;

                    default:
                        goto Label_0299;
                }
            Label_0060:
                this.ExecuteGroupForRowSelection(table, num3, 1, rowsEnum);
                num3++;
            Label_0073:
                //fixed possible bugg in SyscFusion by artem1 due to implement our nested groups (xml, not sql data tables).
                //Because num4 - row count in table, increase after each call to ExecuteGroupForRowSelection(table, num3, 1, rowsEnum) method,
                //that executes for nested group with many items, nested groups in rows in same table shift to bottom and change
                //there row index, so these rows would't be processed and data would't be pooled to table.
                //if (num3 < num4)
                if (num3 < table.Rows.Count)
                {
                    goto Label_0060;
                }
            Label_0299:
                num++;
            }
        }

        //removed by artem1 due to implement our nested groups (xml, not sql data tables).
//        private void ExecuteNestedGroup(string tableName)
//        {
//            IRowsEnumerator rowsEnum = this.GetEnum(tableName);
//            WSection section = null;
//            int num = 0;
//            int count = this.Document.Sections.Count;
//            while (num < count)
//            {
//                section = this.Document.Sections[num];
//                this.ExecuteGroup(section, rowsEnum);
//                num++;
//            }
//            //removed by artem1 due to implement our nested groups (xml, not sql data tables).
//            //this.CurrentDataSet.Tables.Remove(tableName);
//        }

        //removed by artem1 due to implement our nested groups (xml, not sql data tables).
//        public void ExecuteNestedGroup(DbConnection conn, ArrayList commands)
//        {
//            if (conn == null)
//            {
//                throw new ArgumentException("conn");
//            }
//            if (commands == null)
//            {
//                throw new ArgumentException("commands");
//            }
//            this.m_conn = conn;
//            this.m_commands = commands;
//            DictionaryEntry entry = (DictionaryEntry)commands[0];
//            this.Document.SetIsMailMerge(true);
//            this.m_bIsNested = true;
//            this.ExecuteNestedGroup((string)entry.Key);
//            if (this.m_nestedEnums != null)
//            {
//                this.m_nestedEnums.Clear();
//                this.m_nestedEnums = null;
//            }
//            if (this.m_curDataSet != null)
//            {
//                this.m_curDataSet.Clear();
//                this.m_curDataSet = null;
//            }
//            this.Document.SetIsMailMerge(false);
//            this.m_bIsNested = false;
//        }

        //removed by artem1 due to implement our nested groups (xml, not sql data tables).
//        public void ExecuteNestedGroup(DbConnection conn, ArrayList commands, bool isSqlConnection)
//        {
//            this.m_isSqlConnection = isSqlConnection;
//            this.ExecuteNestedGroup(conn, commands);
//        }

        private string FindNestedGroup(int startRow, int endRow, WTable table)
        {
            WTableRow row = null;
            for (int i = startRow; i <= endRow; i++)
            {
                row = table.Rows[i];
                foreach (WTableCell cell in row.Cells)
                {
                    foreach (WParagraph paragraph in cell.Paragraphs)
                    {
                        foreach (ParagraphItem item in paragraph.Items)
                        {
                            if (item is WMergeField)
                            {
                                WMergeField field = item as WMergeField;
                                if (IsBeginGroup(field) && (field.FieldName != string.Empty))
                                {
                                    string fieldName = (item as WMergeField).FieldName;
                                    return ((fieldName == string.Empty) ? null : fieldName);
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private Bitmap GetBitmap(object data)
        {
            if (data.GetType() == typeof(byte[]))
            {
                var stream = new MemoryStream((byte[])data);
                try
                {
                    return new Bitmap(stream);
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        //removed by artem1 due to implement our nested groups (xml, not sql data tables).
//        private string GetCommand(string tableName)
//        {
//            DictionaryEntry entry = new DictionaryEntry(string.Empty, string.Empty);
//            bool flag = false;
//            int num = 0;
//            int count = this.m_commands.Count;
//            while (num < count)
//            {
//                entry = (DictionaryEntry)this.m_commands[num];
//                if (tableName == ((string)entry.Key))
//                {
//                    flag = true;
//                    break;
//                }
//                num++;
//            }
//            if (!flag)
//            {
//                return null;
//            }
//            string command = (string)entry.Value;
//            if (command.IndexOf("%") == -1)
//            {
//                return command;
//            }
//            return this.UpdateVarCmd(command);
//        }

        //removed by artem1 due to implement our nested groups (xml, not sql data tables).
//        private DataTable GetDataTable(string tableName)
//        {
//            DataTable dataTable = null;
//            string cmdText = this.GetCommand(tableName);
//            if (cmdText == null)
//            {
//                cmdText = "Select * from " + tableName;
//            }
//            else if (cmdText == string.Empty)
//            {
//                return null;
//            }
//            dataTable = new DataTable(tableName);
//            DbCommand command = null;
//            DbDataAdapter adapter = null;
//            if (this.m_isSqlConnection)
//            {
//                command = new SqlCommand(cmdText, this.m_conn as SqlConnection);
//                adapter = new SqlDataAdapter(command as SqlCommand);
//            }
//            else
//            {
//                command = new OleDbCommand(cmdText, this.m_conn as OleDbConnection);
//                adapter = new OleDbDataAdapter(command as OleDbCommand);
//            }
//            adapter.Fill(dataTable);
//            return dataTable;
//        }

        private IRowsEnumerator GetEnum(string tableName)
        {
            var tinEnumerator = topEnumerator as ReportDataRangeEnumerator;
            if (tinEnumerator == null) return null;
            return tinEnumerator.GetSubRange(tableName);
            
            //removed by artem1 due to implement our nested groups (xml, not sql data tables).
//            DataTable dataTable = this.GetDataTable(tableName);
//            if (dataTable == null)
//            {
//                return null;
//            }
//            this.CurrentDataSet.Tables.Add(dataTable);
//            IRowsEnumerator enumerator = new DataTableEnumerator(dataTable);
//            enumerator.Reset();
//            return enumerator;
        }

        private void GetFieldNamesForParagraph(ArrayList fieldsArray, TextBodyItem paragraph, string groupName)
        {
            if (paragraph is IWTable)
            {
                WTable table = paragraph as WTable;
                WTableRow row = null;
                WTableCell cell = null;
                TextBodyItem item = null;
                int num = 0;
                int count = table.Rows.Count;
                while (num < count)
                {
                    row = table.Rows[num];
                    int num3 = 0;
                    int num4 = row.Cells.Count;
                    while (num3 < num4)
                    {
                        cell = row.Cells[num3];
                        int num5 = 0;
                        int num6 = cell.Paragraphs.Count;
                        while (num5 < num6)
                        {
                            //item = cell.Items[num5]; fixed internal prop. same as value. artem1
                            item = (TextBodyItem)cell.ChildEntities[num5];
                            this.GetFieldNamesForParagraph(fieldsArray, item, groupName);
                            num5++;
                        }
                        num3++;
                    }
                    num++;
                }
            }
            else
            {
                int num7 = 0;
                int num8 = (paragraph as WParagraph).Items.Count;
                while (num7 < num8)
                {
                    ParagraphItem item2 = (paragraph as WParagraph)[num7];
                    if (item2 is WMergeField)
                    {
                        WMergeField field = item2 as WMergeField;
                        if (field.FieldName == groupName)
                        {
                            if (!this.m_bBeginGroupFound && IsBeginGroup(field))
                            {
                                this.m_bBeginGroupFound = true;
                                this.m_bEndGroupFound = false;
                            }
                            if (!this.m_bEndGroupFound && IsEndGroup(field))
                            {
                                this.m_bEndGroupFound = true;
                                this.m_bBeginGroupFound = false;
                            }
                        }
                        else if ((groupName == null) || (this.m_bBeginGroupFound && !this.m_bEndGroupFound))
                        {
                            fieldsArray.Add(field.FieldName);
                        }
                    }
                    else if (item2 is WTextBox)
                    {
                        WTextBox box = (WTextBox)(paragraph as WParagraph)[num7];
                        //int num9 = box.TextBoxBody.Items.Count; fixed internal prop. same as value. artem1
                        int num9 = box.TextBoxBody.ChildEntities.Count;
                        for (int i = 0; i < num9; i++)
                        {
                            //this.GetFieldNamesForParagraph(fieldsArray, box.TextBoxBody.Items[i], groupName); fixed internal prop. same as value. artem1
                            this.GetFieldNamesForParagraph(fieldsArray, (TextBodyItem)box.TextBoxBody.ChildEntities[i],
                                                           groupName);
                        }
                    }
                    num7++;
                }
            }
        }

        private string GetMappedColName(string fieldName)
        {
            if (this.m_mappedFields != null)
            {
                return (string)this.m_mappedFields[fieldName];
            }
            return null;
        }

        public string[] GetMergeFieldNames()
        {
            ArrayList fieldsArray = new ArrayList();
            this.GetMergeFieldNamesImpl(fieldsArray, null);
            return (string[])fieldsArray.ToArray(typeof(string));
        }

        public string[] GetMergeFieldNames(string groupName)
        {
            ArrayList fieldsArray = new ArrayList();
            this.GetMergeFieldNamesImpl(fieldsArray, groupName);
            return (string[])fieldsArray.ToArray(typeof(string));
        }

        private void GetMergeFieldNamesImpl(ArrayList fieldsArray, string groupName)
        {
            WSection section = null;
            TextBodyItem paragraph = null;
            int num = 0;
            int count = this.Document.Sections.Count;
            while (num < count)
            {
                section = this.Document.Sections[num];
                int num3 = 0;
                //int num4 = section.Body.Items.Count; fixed internal prop. same as value. artem1
                int num4 = section.Body.ChildEntities.Count;
                while (num3 < num4)
                {
                    paragraph = (TextBodyItem)section.Body.ChildEntities[num3];
                    this.GetFieldNamesForParagraph(fieldsArray, paragraph, groupName);
                    num3++;
                }
                for (int i = 0; i < 6; i++)
                {
                    IWParagraphCollection paragraphs = section.HeadersFooters[i].Paragraphs;
                    int num6 = 0;
                    int num7 = paragraphs.Count;
                    while (num6 < num7)
                    {
                        paragraph = paragraphs[num6];
                        this.GetFieldNamesForParagraph(fieldsArray, paragraph, groupName);
                        num6++;
                    }
                }
                num++;
            }
        }

        public string[] GetMergeGroupNames()
        {
            EntityEntry entry;
            ArrayList list = new ArrayList();
            Stack stack = new Stack();
            stack.Push(new EntityEntry(this.Document));
        Label_001D:
            entry = (EntityEntry)stack.Peek();
            if ((entry.Current != null) && entry.Current.IsComposite)
            {
                ICompositeEntity current = entry.Current as ICompositeEntity;
                if (current.ChildEntities.Count > 0)
                {
                    stack.Push(new EntityEntry(current.ChildEntities[0]));
                    goto Label_00D1;
                }
            }
            if ((entry.Current != null) && (entry.Current.EntityType == EntityType.MergeField))
            {
                WMergeField field = entry.Current as WMergeField;
                if (IsBeginGroup(field))
                {
                    list.Add(field.FieldName);
                }
            }
            while (!entry.Fetch())
            {
                stack.Pop();
                if (stack.Count == 0)
                {
                    break;
                }
                entry = (EntityEntry)stack.Peek();
            }
        Label_00D1:
            if (stack.Count > 0)
            {
                goto Label_001D;
            }
            return (string[])list.ToArray(typeof(string));
        }

        private string[] GetMergeGroupNames2()
        {
            IEnumerator enumerator;
            ArrayList list = new ArrayList();
            Stack stack = new Stack();
            stack.Push(this.Document.ChildEntities.GetEnumerator());
        Label_0022:
            enumerator = (IEnumerator)stack.Peek();
            if (enumerator.MoveNext())
            {
                ICompositeEntity current = enumerator.Current as ICompositeEntity;
                if ((current != null) && (current.ChildEntities.Count > 0))
                {
                    stack.Push(current.ChildEntities.GetEnumerator());
                    goto Label_00CF;
                }
            }
            if (enumerator.Current != null)
            {
                Entity entity2 = enumerator.Current as Entity;
                if ((entity2 != null) && (entity2.EntityType == EntityType.MergeField))
                {
                    WMergeField field = entity2 as WMergeField;
                    if (IsBeginGroup(field))
                    {
                        list.Add(field.FieldName);
                    }
                }
            }
            while (!enumerator.MoveNext())
            {
                stack.Pop();
                if (stack.Count == 0)
                {
                    break;
                }
                enumerator = (IEnumerator)stack.Peek();
            }
        Label_00CF:
            if (stack.Count > 0)
            {
                goto Label_0022;
            }
            return (string[])list.ToArray(typeof(string[]));
        }

        private void HideField(IWField field)
        {
            if (!(field as WField).ConvertedToText())
            {
                field.Text = string.Empty;
                (field as WField).SetConvertedToText(true); //fixed
            }
        }

        private void HideFields(BodyItemCollection items)
        {
            TextBodyItem item = null;
            int num = 0;
            int count = items.Count;
            while (num < count)
            {
                //item = items[num];
                item = (TextBodyItem)items[num];
                if (item is WParagraph)
                {
                    this.HideFields(item as WParagraph);
                }
                else if (item is WTable)
                {
                    WTable table = item as WTable;
                    int num3 = 0;
                    int num4 = table.Rows.Count;
                    while (num3 < num4)
                    {
                        this.HideFields(table.Rows[num3]);
                        num3++;
                    }
                }
                num++;
            }
        }

        private void HideFields(IWSectionCollection sections)
        {
            int num = 0;
            int count = sections.Count;
            while (num < count)
            {
                this.ExecuteForSection(sections[num], null);
                num++;
            }
        }

        private void HideFields(WParagraph para)
        {
            WField field = null;
            int num = 0;
            int count = para.Items.Count;
            while (num < count)
            {
                if (para.Items[num] is WField)
                {
                    field = para.Items[num] as WField;
                    if ((field.FieldType == FieldType.FieldMergeField) || (field.FieldType == FieldType.FieldNext))
                    {
                        this.HideField(field);
                    }
                }
                else if (para.Items[num] is WTextBox)
                {
                    //this.HideFields((para.Items[num] as WTextBox).TextBoxBody.Items);
                    this.HideFields((BodyItemCollection)(para.Items[num] as WTextBox).TextBoxBody.ChildEntities);
                }
                num++;
            }
        }

        private void HideFields(WTableRow row)
        {
            WTableCell cell = null;
            int num = 0;
            int count = row.Cells.Count;
            while (num < count)
            {
                cell = row.Cells[num];
                //this.HideFields(cell.Items);
                this.HideFields((BodyItemCollection)cell.ChildEntities);
                num++;
            }
        }

        private static bool IsBeginGroup(WMergeField field)
        {
            string prefix = field.Prefix;
            if (!(prefix == "TableStart"))
            {
                return (prefix == "BeginGroup");
            }
            return true;
        }

        private static bool IsEndGroup(WMergeField field)
        {
            string prefix = field.Prefix;
            if (!(prefix == "TableEnd"))
            {
                return (prefix == "EndGroup");
            }
            return true;
        }

        private void OnBodyGroupFound(IRowsEnumerator rowsEnum)
        {
            GroupSelector groupSelector = this.m_groupSelector;
            TextBodyPart part = new TextBodyPart();
            TextBodySelection groupSelection = groupSelector.GroupSelection;
            part.Copy(groupSelection);
            rowsEnum.Reset();
            while (rowsEnum.NextRow())
            {
                if (this.m_bIsNested)
                {
                    this.UpdateEnum(groupSelector.GroupName, rowsEnum);
                }
                //int count = groupSelection.TextBody.Items.Count;
                int count = groupSelection.TextBody.ChildEntities.Count;
                this.ExecuteGroupForSelection(groupSelection.TextBody, groupSelection.ItemStartIndex,
                                              groupSelection.ItemEndIndex, groupSelection.ParagraphItemStartIndex,
                                              groupSelection.ParagraphItemEndIndex, rowsEnum);
                //groupSelection.ItemEndIndex += groupSelection.TextBody.Items.Count - count;
                groupSelection.ItemEndIndex += groupSelection.TextBody.ChildEntities.Count - count;
                if (rowsEnum.IsLast)
                {
                    if (this.m_bIsNested)
                    {
                        this.NestedEnums.Remove(groupSelector.GroupName);
                        return;
                    }
                    break;
                }
                part.PasteAt(groupSelection.TextBody, groupSelection.ItemEndIndex, groupSelection.ParagraphItemEndIndex);
                int pEndIndexShift = 0;
                if ((part.BodyItems.Count == 1) && (part.BodyItems[0] is WParagraph))
                {
                    pEndIndexShift = (part.BodyItems[0] as WParagraph).Items.Count - 1;
                }


                //added by artem1
                //Если диапазон включает в себя таблицу (испльзуемую просто для текста), и эта таблица
                //копируется вместе с окружающим ее текстом, то объединение ячеек в таких таблицах не работает
                //по той же причине, по которой в методе OnRowGroupFound переписан кусок клонирования строк таблицы.
                //Поэтому пришлось изобретать такой обходной ход.
                //Замечание: если в дебаге исследовать свойства строк и ячеек, копируемых в методе PasteAt (см.выше),
                //то в какой то момент все начинает нормально работать и без этих обходных путей. Но свойство, от которого
                //это зависит, так и не найдено.
                /*for (int i = groupSelection.ItemEndIndex; i < groupSelection.ItemEndIndex + part.BodyItems.Count; i++)
                {
                    var entity = groupSelection.TextBody.ChildEntities[i];
                    if (entity is WTable)
                    {
                        var table = (WTable)entity;
                        
                        if (table.Rows.Count > 0)
                        {
                            var srcFirstRow = 0;
                            var srcLastRow = table.Rows.Count - 1;
                            for (var srcRowIndex = srcFirstRow; srcRowIndex <= srcLastRow; srcRowIndex++)
                            {
                                var srcRow = table.Rows[srcRowIndex];
                                table.AddRowAt(srcRow, table.Rows.Count);
                            }

                            for (var srcRowIndex = srcFirstRow; srcRowIndex <= srcLastRow; srcRowIndex++)
                            {
                                var srcRow = table.Rows[srcRowIndex];
                                var newRow = table.Rows[srcRowIndex + srcLastRow + 1];

                                CopyRowFormat(srcRow.RowFormat, newRow.RowFormat);
                                for (var srcCellIndex = 0; srcCellIndex < srcRow.Cells.Count; srcCellIndex++)
                                {
                                    var srcCell = srcRow.Cells[srcCellIndex];
                                    var newCell = newRow.Cells[srcCellIndex];
                                    CopyCellFormat(srcCell.CellFormat, newCell.CellFormat);
                                }
                            }

                            for (var delRowIndex = srcLastRow; delRowIndex >= srcFirstRow; delRowIndex--)
                            {
                                table.Rows.RemoveAt(0);
                            }
                        }
                    }
                }*/
                //end added by artem1

                groupSelection.ShiftStartToEnd(part.BodyItems.Count - 1, pEndIndexShift);
            }
        }

        private void OnGroupFound(IRowsEnumerator rowsEnum)
        {
            GroupSelector groupSelector = this.m_groupSelector;
            this.HideField(groupSelector.BeginGroupField);
            this.HideField(groupSelector.EndGroupField);
            if (this.m_bIsNested)
            {
                this.m_groupSelector.BeginGroupField.FieldName = string.Empty;
                this.m_groupSelector.EndGroupField.FieldName = string.Empty;
            }
            if (this.CheckSelection(rowsEnum))
            {
                if (groupSelector.GroupSelection != null)
                {
                    this.OnBodyGroupFound(rowsEnum);
                }
                else if (groupSelector.RowSelection != null)
                {
                    this.OnRowGroupFound(rowsEnum);
                }

                if (RangeInTableRendered != null)
                {
                    var ourEnumerator = (rowsEnum as ReportDataRangeEnumerator);
                    string range = ourEnumerator != null ? ourEnumerator.TableName : null;
                    var rangeNode = ourEnumerator != null ? ourEnumerator.RangeNode : null;

                    RangeInTableRendered(this, new RangeInTableRenderedEventArgs(range, rangeNode));
                }
            }
        }

        private void OnRowGroupFound(IRowsEnumerator rowsEnum)
        {
            GroupSelector groupSelector = this.m_groupSelector;
            WTable table = groupSelector.RowSelection.Table;
            int startRowIndex = groupSelector.RowSelection.StartRowIndex;
            int endRowIndex = groupSelector.RowSelection.EndRowIndex;
            int count = table.Rows.Count;
            int num3 = startRowIndex;
            int num4 = 0;
            if (this.m_bIsNested)
            {
                this.VerifyNestedGroups(startRowIndex, endRowIndex, table);
            }
            int num5 = (endRowIndex - startRowIndex) + 1;
            WTableRow[] rowArray = new WTableRow[num5];
            int index = 0;
            for (int i = startRowIndex; i <= endRowIndex; i++)
            {
                rowArray[index] = table.Rows[i].Clone();
                index++;
            }
            rowsEnum.Reset();
            while (rowsEnum.NextRow())
            {
                if (this.m_bIsNested)
                {
                    this.UpdateEnum(groupSelector.GroupName, rowsEnum);
                }
                num4 = this.ExecuteGroupForRowSelection(table, num3, num5, rowsEnum);
                if (rowsEnum.IsLast)
                {
                    break;
                }
                num3 += num4;
                for (int j = 0; j < num5; j++)
                {
                    table.Rows.Insert(num3 + j, rowArray[j].Clone());
                }
            }
            groupSelector.RowSelection.StartRowIndex = num3;
        }

        public event EventHandler<RangeInTableRenderedEventArgs> RangeInTableRendered;

        /*
         * Использовалось, когда руками копировали строки из-за каких то проблем с последующим
         * объединением ячеек. Сейчас, с новой версией syncfusion(8.3) нормально объединяется и так,
         * поэтому методы временно отключены, но не удалены. Если возникнет проблемы с неправильным
         * копированием форматов, эти методы могут пригодиться
        private static void CopyCellFormat(CellFormat from, CellFormat to)
        {
            //to.BackColor = from.BackColor;
            to.FitText = from.FitText;
            to.HorizontalMerge = from.HorizontalMerge;
            to.SamePaddingsAsTable = from.SamePaddingsAsTable;
            if (!to.SamePaddingsAsTable)
            {
                to.Paddings.Left = from.Paddings.Left;
                to.Paddings.Top = from.Paddings.Top;
                to.Paddings.Right = from.Paddings.Right;
                to.Paddings.Bottom = from.Paddings.Bottom;
            }
            to.TextDirection = from.TextDirection;
            to.TextWrap = from.TextWrap;
            to.VerticalAlignment = from.VerticalAlignment;
            to.VerticalMerge = from.VerticalMerge;
            
            
            CopyBorders(from.Borders.Left, to.Borders.Left);
            CopyBorders(from.Borders.Top, to.Borders.Top);
            CopyBorders(from.Borders.Right, to.Borders.Right);
            CopyBorders(from.Borders.Bottom, to.Borders.Bottom);
            CopyBorders(from.Borders.Horizontal, to.Borders.Horizontal);
            CopyBorders(from.Borders.Vertical, to.Borders.Vertical);
        }

        private static void CopyRowFormat(RowFormat from, RowFormat to)
        {
            to.LeftIndent = from.LeftIndent;
            //to.BackColor = from.BackColor;
            to.Bidi = from.Bidi;
            to.CellSpacing = from.CellSpacing;
            to.HorizontalAlignment = from.HorizontalAlignment;
            to.IsAutoResized = from.IsAutoResized;
            to.IsBreakAcrossPages = from.IsBreakAcrossPages;

            to.Paddings.Left = from.Paddings.Left;
            to.Paddings.Top = from.Paddings.Top;
            to.Paddings.Right = from.Paddings.Right;
            to.Paddings.Bottom = from.Paddings.Bottom;
            
            CopyBorders(from.Borders.Left, to.Borders.Left);
            CopyBorders(from.Borders.Top, to.Borders.Top);
            CopyBorders(from.Borders.Right, to.Borders.Right);
            CopyBorders(from.Borders.Bottom, to.Borders.Bottom);
            CopyBorders(from.Borders.Horizontal, to.Borders.Horizontal);
            CopyBorders(from.Borders.Vertical, to.Borders.Vertical);
        }

        private static void CopyBorders(Border from, Border to)
        {
            to.InitFormatting(from.Color, from.LineWidth, from.BorderType, from.Shadow);
            to.Space = from.Space;
        }
        */

        private void RemoveEmptyPara(BodyItemCollection items)
        {
            TextBodyItem item = null;
            int paraIndex = 0;
            int count = items.Count;
            while (paraIndex < count)
            {
                //item = items[paraIndex];
                item = (TextBodyItem)items[paraIndex];
                if (item is WParagraph)
                {
                    if (this.RemoveEmptyPara(items, paraIndex))
                    {
                        paraIndex--;
                        count--;
                    }
                }
                else if (item is WTable)
                {
                    WTable table = item as WTable;
                    int num3 = 0;
                    int num4 = table.Rows.Count;
                    while (num3 < num4)
                    {
                        this.RemoveEmptyPara(table.Rows[num3]);
                        num3++;
                    }
                }
                paraIndex++;
            }
        }

        private void RemoveEmptyPara(WParagraph para)
        {
            if ((para.Items.Count > 0) && (para.Items[0] is WMergeField))
            {
                //para.RemoveEmpty = true;
                para.SetRemoveEmpty(true);
            }
        }

        private void RemoveEmptyPara(WTableRow row)
        {
            WTableCell cell = null;
            int num = 0;
            int count = row.Cells.Count;
            while (num < count)
            {
                cell = row.Cells[num];
                //this.RemoveEmptyPara(cell.Items);
                this.RemoveEmptyPara((BodyItemCollection)cell.ChildEntities);
                num++;
            }
        }

        private bool RemoveEmptyPara(BodyItemCollection paragraphs, int paraIndex)
        {
            WParagraph entity = paragraphs[paraIndex] as WParagraph;
            if (((entity.Items.Count > 0) && (entity.Text == string.Empty)) && (entity.Items[0] is WMergeField))
            {
                paragraphs.Remove(entity);
                return true;
            }
            return false;
        }

        protected MergeFieldEventArgs SendMergeField(IWMergeField field, object value, IRowsEnumerator rowsEnum)
        {
            MergeFieldEventArgs args = new MergeFieldEventArgs(this.Document, 
                rowsEnum.TableName, rowsEnum.CurrentRowIndex, field, value);
            if (this.MergeField != null)
            {
                this.MergeField(this, args);
            }
            return args;
        }

        //added by artem1 due to implement our conditional formatting
        protected MergeFieldEventArgs SendMergeFieldFixed(IWMergeField field, object value, IRowsEnumerator rowsEnum, XmlElement valueNode)
        {
            var args = new FieldRenderedEventArgs(this.Document, 
                rowsEnum.TableName, rowsEnum.CurrentRowIndex, field, value, valueNode);

            if (this.MergeField != null)
            {
                this.MergeField(this, args);
            }
            return args;
        }

        protected MergeImageFieldEventArgs SendMergeImageField(IWMergeField field, object bmp, IRowsEnumerator rowsEnum)
        {
            MergeImageFieldEventArgs args = null;
            if (rowsEnum != null)
            {
                args = new MergeImageFieldEventArgs(this.Document, rowsEnum.TableName, rowsEnum.CurrentRowIndex, field,
                                                    bmp);
            }
            else
            {
                args = new MergeImageFieldEventArgs(this.Document, null, 0x7fffffff, field, bmp);
            }
            if (this.MergeImageField != null)
            {
                this.MergeImageField(this, args);
            }
            return args;
        }

        private void UpdateEnum(string tableName, IRowsEnumerator rowsEnum)
        {
            if (this.NestedEnums[tableName] == null)
            {
                this.NestedEnums.Add(tableName, rowsEnum);
            }
            else
            {
                this.NestedEnums[tableName] = rowsEnum;
            }
        }

        private void UpdateFieldValue(IWMergeField field)
        {
            if (this.m_bClearFields && (this.m_values == null))
            {
                field.Text = "";
                //(field as WMergeField).ConvertedToText = true;
                (field as WMergeField).SetConvertedToText(true);
            }
            else
            {
                int rowIndex = -1;
                string mappedColName = this.GetMappedColName(field.FieldName);
                if (mappedColName != null)
                {
                    for (int i = 0; i < this.m_names.Length; i++)
                    {
                        if (this.m_names[i].ToUpper() == mappedColName.ToUpper())
                        {
                            rowIndex = i;
                            break;
                        }
                    }
                }
                if (rowIndex == -1)
                {
                    for (int j = 0; j < this.m_names.Length; j++)
                    {
                        if (this.m_names[j].ToUpper() == field.FieldName.ToUpper())
                        {
                            rowIndex = j;
                            break;
                        }
                    }
                }
                if (rowIndex != -1)
                {
                    MergeFieldEventArgs args = new MergeFieldEventArgs(this.Document, "", rowIndex, field,
                                                                       this.m_values[rowIndex]);
                    if (this.MergeField != null)
                    {
                        this.MergeField(this, args);
                    }
                    field.Text = args.Text;
                    //(field as WMergeField).ConvertedToText = true;
                    (field as WMergeField).SetConvertedToText(true);
                }
                else if (this.m_bClearFields)
                {
                    this.HideField(field);
                }
            }
        }

        private void UpdateFieldValue(IWMergeField field, IRowsEnumerator rowsEnum)
        {
            if (rowsEnum == null)
            {
                this.UpdateFieldValue(field);
            }
            else
            {
                string columnName = null;
                bool flag = false;
                object cellValue = null;
                columnName = this.GetMappedColName(field.FieldName);

                if (columnName != null)
                {
                    cellValue = rowsEnum.GetCellValue(columnName);
                }
                if (cellValue == null)
                {
                    int index = 0;
                    int length = rowsEnum.ColumnNames.Length;
                    while (index < length)
                    {
                        columnName = rowsEnum.ColumnNames[index];
                        string str2 = field.FieldName.ToUpper();
                        string str3 = columnName.ToUpper();
                        if ((str2 == str3) || (str2 == ("\"" + str3 + "\"")))
                        {
                            cellValue = rowsEnum.GetCellValue(columnName);
                            break;
                        }
                        index++;
                    }
                }
                if (cellValue != null)
                {
                    //added by artem1 due to implementing conditional formatting
                    XmlElement valueNode = null;
                    var ourEnumerator = rowsEnum as ReportDataRangeEnumerator;
                    if (ourEnumerator != null)
                    {
                        valueNode = (XmlElement)ourEnumerator.GetValueNode(columnName);
                    }

                    MergeFieldEventArgs args = valueNode != null
                       ? this.SendMergeFieldFixed(field, cellValue, rowsEnum, valueNode)
                       : this.SendMergeField(field, cellValue, rowsEnum);
                    field.Text = args.Text;
                    //(field as WMergeField).ConvertedToText = true;
                    (field as WMergeField).SetConvertedToText(true);
                    flag = true;
                }
                if (!flag && this.m_bClearFields)
                {
                    this.HideField(field);
                }
            }
        }

        private void UpdateIfFieldValue(WIfField field, IRowsEnumerator rowsEnum)
        {
            //if ((field.MergeFields.Count != 0) && (rowsEnum != null))
            if ((field.GetMergeFields().Count != 0) && (rowsEnum != null))
            {
                string columnName = null;
                int index = 0;
                int length = rowsEnum.ColumnNames.Length;
                while (index < length)
                {
                    columnName = rowsEnum.ColumnNames[index];
                    string str2 = columnName.ToUpper();
                    PseudoMergeField field2 = null;
                    int num3 = 0;
                    //int count = field.MergeFields.Count;
                    int count = field.GetMergeFields().Count;
                    while (num3 < count)
                    {
                        //field2 = field.MergeFields[num3] as PseudoMergeField;
                        field2 = field.GetMergeFields()[num3] as PseudoMergeField;
                        if ((field2.Name != null) && (field2.Name.ToUpper() == str2))
                        {
                            field2.Value = rowsEnum.GetCellValue(columnName).ToString();
                        }
                        num3++;
                    }
                    index++;
                }
            }
        }

        private void UpdateImageFieldValue(IWMergeField field, IWParagraph paragraph, IRowsEnumerator rowsEnum)
        {
            if ((rowsEnum == null) && (this.MergeImageField == null))
            {
                this.UpdateFieldValue(field);
            }
            else
            {
                MergeImageFieldEventArgs args = null;
                if ((rowsEnum == null) && (this.MergeImageField != null))
                {
                    args = this.SendMergeImageField(field, null, rowsEnum);
                    this.UpdateMergedPicture(field, paragraph, args);
                }
                else
                {
                    bool flag = false;
                    string fieldName = field.FieldName;
                    string columnName = null;
                    object data = null;
                    columnName = this.GetMappedColName(fieldName);
                    if (columnName != null)
                    {
                        data = rowsEnum.GetCellValue(columnName);
                    }
                    if (data == null)
                    {
                        int index = 0;
                        int length = rowsEnum.ColumnNames.Length;
                        while (index < length)
                        {
                            columnName = rowsEnum.ColumnNames[index];
                            if (columnName.ToUpper() == fieldName.ToUpper())
                            {
                                data = rowsEnum.GetCellValue(columnName);
                                break;
                            }
                            index++;
                        }
                    }
                    if (data != null)
                    {
                        Bitmap bitmap = this.GetBitmap(data);
                        if (bitmap != null)
                        {
                            data = bitmap;
                        }
                        args = this.SendMergeImageField(field, data, rowsEnum);
                        this.UpdateMergedPicture(field, paragraph, args);
                        flag = true;
                    }
                    if (!flag && this.m_bClearFields)
                    {
                        this.HideField(field);
                    }
                }
            }
        }

        private void UpdateMergedPicture(IWMergeField field, IWParagraph paragraph, MergeImageFieldEventArgs args)
        {
            if (args.UseText)
            {
                field.Text = args.Text;
            }
            else
            {
                int index = paragraph.Items.IndexOf(field);
                paragraph.Items.RemoveAt(index);
                IWPicture entity = (IWPicture)this.Document.CreateParagraphItem(ParagraphItemType.Picture);
                paragraph.Items.Insert(index, entity);
                if (args.Image != null)
                {
                    entity.LoadImage(args.Image);
                }
            }
        }

        //removed by artem1 due to implement our nested groups (xml, not sql data tables).
//        private string UpdateVarCmd(string command)
//        {
//            MatchCollection matchs = this.VariableCommandRegex.Matches(command);
//            if (matchs.Count == 0)
//            {
//                return null;
//            }
//            char[] separator = new char[] { '.' };
//            string newValue = null;
//            string str2 = null;
//            string[] strArray = null;
//            int num = 0;
//            int count = matchs.Count;
//            while (num < count)
//            {
//                str2 = matchs[num].Value.Replace("%", string.Empty);
//                strArray = str2.Split(separator);
//                if (strArray.Length != 2)
//                {
//                    throw new ArgumentException("String value between '%' symbols (variable command) is not valid.");
//                }
//                IRowsEnumerator enumerator = this.NestedEnums[strArray[0]] as IRowsEnumerator;
//                if (enumerator == null)
//                {
//                    return string.Empty;
//                }
//                newValue = enumerator.GetCellValue(strArray[1]).ToString();
//                command = command.Replace("%" + str2 + "%", newValue);
//                num++;
//            }
//            return command;
//        }

        private void VerifyNestedGroups(int startRow, int endRow, WTable table)
        {
            HybridDictionary dictionary = new HybridDictionary();
            HybridDictionary dictionary2 = new HybridDictionary();
            WTableRow row = null;
            WMergeField field = null;
            for (int i = startRow; i <= endRow; i++)
            {
                row = table.Rows[i];
                foreach (WTableCell cell in row.Cells)
                {
                    foreach (WParagraph paragraph in cell.Paragraphs)
                    {
                        foreach (ParagraphItem item in paragraph.Items)
                        {
                            if (item is WMergeField)
                            {
                                field = item as WMergeField;
                                if (IsBeginGroup(field) && !field.ConvertedToText()) /* fixed with extension method */
                                {
                                    dictionary.Add(field.FieldName, field);
                                }
                                else if (IsEndGroup(field) && !field.ConvertedToText()) /* fixed with extension method */
                                {
                                    dictionary2.Add(field.FieldName, field);
                                }
                            }
                        }
                    }
                }
            }
            if (dictionary.Count == 0)
            {
                if (dictionary2.Count > 0)
                {
                    foreach (DictionaryEntry entry in dictionary2)
                    {
                        throw new ApplicationException("GroupEnd field \"" + entry.Key.ToString() +
                                                       "\" doesn't have GroupStart field equivalent.");
                    }
                }
            }
            else if ((dictionary2.Count == 0) && (dictionary.Count > 0))
            {
                foreach (DictionaryEntry entry2 in dictionary)
                {
                    throw new ApplicationException("GroupStart field \"" + entry2.Key.ToString() +
                                                   "\" doesn't have GroupEnd field equivalent.");
                }
            }
            foreach (DictionaryEntry entry3 in dictionary)
            {
                string key = (string)entry3.Key;
                if (dictionary2[key] == null)
                {
                    throw new ApplicationException("GroupStart field \"" + key +
                                                   "\" doesn't have GroupEnd field equivalent.");
                }
                dictionary2.Remove(key);
            }
            if (dictionary2.Count > 0)
            {
                foreach (DictionaryEntry entry4 in dictionary2)
                {
                    throw new ApplicationException("GroupEnd field \"" + entry4.Key.ToString() +
                                                   "\" doesn't have GroupStart field equivalent.");
                }
            }
            dictionary.Clear();
            dictionary2.Clear();
        }

        // Properties
        public bool ClearFields
        {
            get { return this.m_bClearFields; }
            set { this.m_bClearFields = value; }
        }

        //removed by artem1 due to implement our nested groups (xml, not sql data tables).
//        private DataSet CurrentDataSet
//        {
//            get
//            {
//                if (this.m_curDataSet == null)
//                {
//                    this.m_curDataSet = new DataSet();
//                }
//                return this.m_curDataSet;
//            }
//        }

        protected WordDocument Document
        {
            get { return this.m_doc; }
        }

        private Stack GroupSelectors
        {
            get
            {
                if (this.m_groupSelectors == null)
                {
                    this.m_groupSelectors = new Stack();
                }
                return this.m_groupSelectors;
            }
        }

        public Hashtable MappedFields
        {
            get
            {
                if (this.m_mappedFields == null)
                {
                    this.m_mappedFields = new Hashtable();
                }
                return this.m_mappedFields;
            }
        }

        private HybridDictionary NestedEnums
        {
            get
            {
                if (this.m_nestedEnums == null)
                {
                    this.m_nestedEnums = new HybridDictionary();
                }
                return this.m_nestedEnums;
            }
        }

        public bool RemoveEmptyParagraphs
        {
            get { return this.m_bRemoveEmptyPara; }
            set { this.m_bRemoveEmptyPara = value; }
        }

        //removed by artem1 due to implement our nested groups (xml, not sql data tables).
//        private Regex VariableCommandRegex
//        {
//            get
//            {
//                if (this.m_varCmdRegex == null)
//                {
//                    this.m_varCmdRegex = new Regex("%([^\"%]+)%");
//                }
//                return this.m_varCmdRegex;
//            }
//        }

        // Nested Types
//        internal class GroupSelector
//        {
//            // Fields
//            private WMergeField m_beginGroupField;
//            private WTextBody m_body;
//            private int m_bodyItemIndex;
//            private int m_bodyItemStartIndex = -1;
//            private WMergeField m_endGroupField;
//            private string m_groupName;
//            private TextBodySelection m_groupSelection;
//            private WTextBody m_groupTextBody;
//            private int m_paragraphItemIndex = -1;
//            private int m_paragraphItemStartIndex = -1;
//            private int m_rowIndex = -1;
//            private TIFixedMailMerge.TableRowSelection m_rowSelection;
//            private IRowsEnumerator m_rowsEnum;
//            private int m_selBodyItemsCnt = -1;
//            private int m_startRowIndex = -1;
//            private GroupFound SendGroupFound;
//
//            // Methods
//            internal GroupSelector(GroupFound onGroupFound)
//            {
//                this.SendGroupFound = (GroupFound)Delegate.Combine(this.SendGroupFound, onGroupFound);
//            }
//
//            private void CheckItem(ParagraphItem item)
//            {
//                if (item.EntityType == EntityType.MergeField)
//                {
//                    WMergeField field = item as WMergeField;
//                    if (field.FieldName == this.m_groupName)
//                    {
//                        if (this.m_beginGroupField == null)
//                        {
//                            if (TIFixedMailMerge.IsBeginGroup(field))
//                            {
//                                this.StartSelection(field);
//                            }
//                        }
//                        else if (TIFixedMailMerge.IsEndGroup(field))
//                        {
//                            this.EndSelection(field);
//                            if (this.SendGroupFound != null)
//                            {
//                                this.SendGroupFound(this.m_rowsEnum);
//                            }
//                        }
//                    }
//                }
//            }
//
//            private void ClearSelection()
//            {
//                this.m_groupSelection = null;
//                this.m_rowSelection = null;
//                this.m_beginGroupField = null;
//                this.m_endGroupField = null;
//            }
//
//            private void EndSelection(WMergeField field)
//            {
//                this.m_endGroupField = field;
//                WTextBody ownerTextBody = field.OwnerParagraph.OwnerTextBody;
//                this.m_selBodyItemsCnt = (this.m_bodyItemIndex - this.m_bodyItemStartIndex) + 1;
//                if (ownerTextBody == this.m_groupTextBody)
//                {
//                    this.m_groupSelection = new TextBodySelection(ownerTextBody, this.m_bodyItemStartIndex,
//                                                                  this.m_bodyItemIndex, this.m_paragraphItemStartIndex,
//                                                                  this.m_paragraphItemIndex);
//                }
//                else
//                {
//                    //                if (((ownerTextBody.EntityType != EntityType.TableCell) || (this.m_groupTextBody.EntityType != EntityType.TableCell)) || ((this.m_groupTextBody.Owner as WTableRow).OwnerTable != (ownerTextBody.Owner as WTableRow).OwnerTable))
//                    //                {
//                    //                    throw new MailMergeException();
//                    //                }
//                    //fixed internal prop - use same private member. artem1
//                    if (((ownerTextBody.EntityType != EntityType.TableCell) ||
//                         (this.m_groupTextBody.EntityType != EntityType.TableCell)) ||
//                        (((this.m_groupTextBody.Owner as WTableRow).Owner as WTable) !=
//                         ((ownerTextBody.Owner as WTableRow).Owner as WTable)))
//                    {
//                        throw new MailMergeException();
//                    }
//                    this.UpdateEndSelection(ownerTextBody as WTableCell);
//                    this.m_rowSelection = new TIFixedMailMerge.TableRowSelection(ownerTextBody.Owner.Owner as WTable,
//                                                                                 this.m_startRowIndex, this.m_rowIndex);
//                }
//            }
//
//            private void FindInBodyItems(BodyItemCollection bodyItems)
//            {
//                int itemEndIndex = 0;
//                int count = bodyItems.Count;
//                while (itemEndIndex < count)
//                {
//                    WParagraph paragraph;
//                    int num3;
//                    int num4;
//                    ParagraphItem item2;
//                    //TextBodyItem item = bodyItems[itemEndIndex]; Items from BodyItemsCollection with correct indexer is internal. artem1
//                    TextBodyItem item = (TextBodyItem)bodyItems[itemEndIndex];
//                    this.m_bodyItemIndex = itemEndIndex;
//                    switch (item.EntityType)
//                    {
//                        case EntityType.Paragraph:
//                            paragraph = (WParagraph)item;
//                            num3 = 0;
//                            num4 = paragraph.Items.Count;
//                            goto Label_00F4;
//
//                        case EntityType.Table:
//                            {
//                                WTable table = (WTable)item;
//                                this.FindInTable(table, 0, table.Rows.Count - 1);
//                                goto Label_0126;
//                            }
//                        default:
//                            throw new Exception();
//                    }
//                Label_0057:
//                    item2 = paragraph.Items[num3];
//                    if ((item2 is BookmarkStart) || (item2 is BookmarkEnd))
//                    {
//                        paragraph.Items.RemoveAt(num3);
//                        num3--;
//                        //not in new version??? num4 = paragraph.Items.Count;
//                    }
//                    else
//                    {
//                        this.m_paragraphItemIndex = num3;
//                        if (item2 is WTextBox)
//                        {
//                            //this.FindInBodyItems((item2 as WTextBox).TextBoxBody.Items); fixed internal prop - use same private member. artem1
//                            this.FindInBodyItems((BodyItemCollection)(item2 as WTextBox).TextBoxBody.ChildEntities);
//                        }
//                        else
//                        {
//                            this.CheckItem(item2);
//                        }
//                        if (this.IsGroupFound)
//                        {
//                            if (this.m_groupSelection == null)
//                            {
//                                goto Label_0126;
//                            }
//                            itemEndIndex = this.m_groupSelection.ItemEndIndex;
//                            this.ClearSelection();
//                        }
//                    }
//                    num3++;
//                Label_00F4:
//                    if (num3 < num4)
//                    {
//                        goto Label_0057;
//                    }
//                Label_0126:
//                    itemEndIndex++;
//                }
//            }
//
//            private void FindInTable(WTable table, int startRow, int endRow)
//            {
//                int count = table.Rows.Count;
//                for (int i = startRow; i <= endRow; i++)
//                {
//                    WTableRow row = table.Rows[i];
//                    this.m_rowIndex = i;
//                    int num3 = 0;
//                    int num4 = row.Cells.Count;
//                    while (num3 < num4)
//                    {
//                        WTableCell cell = row.Cells[num3];
//                        //this.FindInBodyItems(cell.Items); fixed internal prop - use same private member. artem1
//                        this.FindInBodyItems((BodyItemCollection)cell.ChildEntities);
//                        if (this.IsGroupFound)
//                        {
//                            endRow += table.Rows.Count - count;
//                            i = this.m_rowSelection.StartRowIndex;
//                            this.ClearSelection();
//                            break;
//                        }
//                        num3++;
//                    }
//                }
//            }
//
//            private void InitProcess(IRowsEnumerator rowsEnum)
//            {
//                this.m_groupSelection = null;
//                this.m_rowSelection = null;
//                this.m_beginGroupField = null;
//                this.m_endGroupField = null;
//                this.m_bodyItemIndex = 0;
//                this.m_bodyItemStartIndex = -1;
//                this.m_paragraphItemIndex = -1;
//                this.m_paragraphItemStartIndex = -1;
//                this.m_rowIndex = -1;
//                this.m_selBodyItemsCnt = -1;
//                this.m_rowsEnum = rowsEnum;
//                this.m_groupName = this.m_rowsEnum.TableName;
//            }
//
//            internal void ProcessGroups(WTextBody body, IRowsEnumerator rowsEnum)
//            {
//                this.InitProcess(rowsEnum);
//                this.m_groupTextBody = this.m_body = body;
//                //this.FindInBodyItems(this.m_body.Items); //fixed internal prop - use same private member. artem1
//                this.FindInBodyItems((BodyItemCollection)this.m_body.ChildEntities);
//            }
//
//            internal void ProcessGroups(WTable table, int startRow, int endRow, IRowsEnumerator rowsEnum)
//            {
//                this.InitProcess(rowsEnum);
//                this.FindInTable(table, startRow, endRow);
//            }
//
//            private void StartSelection(WMergeField field)
//            {
//                this.m_beginGroupField = field;
//                this.m_groupTextBody = field.OwnerParagraph.OwnerTextBody;
//                this.m_bodyItemStartIndex = this.m_bodyItemIndex;
//                this.m_paragraphItemStartIndex = this.m_paragraphItemIndex;
//                this.m_startRowIndex = this.m_rowIndex;
//            }
//
//            private void UpdateEndSelection(WTableCell cell)
//            {
//                WTableRow ownerRow = cell.OwnerRow;
//                bool flag = false;
//                //begin fixed not impliciti casting IEnumerator to IDisposable
//                IEnumerator enumerator = null;
//                try
//                {
//                    enumerator = ownerRow.Cells.GetEnumerator();
//                    while (enumerator.MoveNext())
//                    {
//                        WTableCell current = (WTableCell)enumerator.Current;
//                        if (cell.CellFormat.VerticalMerge != CellMerge.None)
//                        {
//                            flag = true;
//                            goto Label_004F;
//                        }
//                    }
//                }
//                finally
//                {
//                    var disposable = enumerator as IDisposable;
//                    if (disposable != null) disposable.Dispose();
//                }
//            /*
//        using (IEnumerator enumerator = ownerRow.Cells.GetEnumerator())
//        {
//            while (enumerator.MoveNext())
//            {
//                WTableCell current = (WTableCell) enumerator.Current;
//                if (cell.CellFormat.VerticalMerge != CellMerge.None)
//                {
//                    flag = true;
//                    goto Label_004F;
//                }
//            }
//        }
//        */
//            //edd
//            Label_004F:
//                if (flag)
//                {
//                    goto Label_00BF;
//                }
//                return;
//            Label_00AE:
//                if (!flag)
//                {
//                    return;
//                }
//                this.m_rowIndex++;
//            Label_00BF:
//                if (ownerRow.NextSibling != null)
//                {
//                    ownerRow = ownerRow.NextSibling as WTableRow;
//                    flag = false;
//                    IEnumerator enumerator2 = null;
//                    try
//                    {
//                        enumerator2 = ownerRow.Cells.GetEnumerator();
//                        while (enumerator2.MoveNext())
//                        {
//                            WTableCell cell2 = (WTableCell)enumerator2.Current;
//                            if (cell.CellFormat.VerticalMerge != CellMerge.None)
//                            {
//                                flag = true;
//                                goto Label_00AE;
//                            }
//                        }
//                    }
//                    finally
//                    {
//                        var disposable = enumerator2 as IDisposable;
//                        if (disposable != null) disposable.Dispose();
//                    }
//                    /*
//                using (IEnumerator enumerator2 = ownerRow.Cells.GetEnumerator())
//                {
//                    while (enumerator2.MoveNext())
//                    {
//                        WTableCell cell2 = (WTableCell) enumerator2.Current;
//                        if (cell.CellFormat.VerticalMerge != CellMerge.None)
//                        {
//                            flag = true;
//                            goto Label_00AE;
//                        }
//                    }
//                }
//                */
//                    goto Label_00AE;
//                }
//            }
//
//            // Properties
//            internal WMergeField BeginGroupField
//            {
//                get { return this.m_beginGroupField; }
//            }
//
//            internal int BodyItemIndex
//            {
//                get { return this.m_bodyItemIndex; }
//                set { this.m_bodyItemIndex = value; }
//            }
//
//            internal WMergeField EndGroupField
//            {
//                get { return this.m_endGroupField; }
//                set { this.m_endGroupField = value; }
//            }
//
//            internal string GroupName
//            {
//                get { return this.m_groupName; }
//            }
//
//            internal TextBodySelection GroupSelection
//            {
//                get { return this.m_groupSelection; }
//            }
//
//            internal bool IsGroupFound
//            {
//                get { return (this.m_endGroupField != null); }
//            }
//
//            internal TIFixedMailMerge.TableRowSelection RowSelection
//            {
//                get { return this.m_rowSelection; }
//            }
//
//            internal int SelectedBodyItemsCount
//            {
//                get { return this.m_selBodyItemsCnt; }
//            }
//
//            // Nested Types
//            internal delegate void GroupFound(IRowsEnumerator rowsEnum);
//        }

        internal class GroupSelector
        {
            // Fields
            private WMergeField m_beginGroupField;
            private WTextBody m_body;
            private int m_bodyItemIndex;
            private int m_bodyItemStartIndex = -1;
            private WMergeField m_endGroupField;
            private string m_groupName;
            private TextBodySelection m_groupSelection;
            private WTextBody m_groupTextBody;
            private int m_paragraphItemIndex = -1;
            private int m_paragraphItemStartIndex = -1;
            private int m_rowIndex = -1;
            private TableRowSelection m_rowSelection;
            private IRowsEnumerator m_rowsEnum;
            private int m_selBodyItemsCnt = -1;
            private int m_startRowIndex = -1;
            private GroupFound SendGroupFound;

            // Methods
            internal GroupSelector(GroupFound onGroupFound)
            {
                this.SendGroupFound = (GroupFound)Delegate.Combine(this.SendGroupFound, onGroupFound);
            }

            private void CheckItem(ParagraphItem item)
            {
                if (item.EntityType == EntityType.MergeField)
                {
                    WMergeField field = item as WMergeField;
                    if (field.FieldName == this.m_groupName)
                    {
                        if (this.m_beginGroupField == null)
                        {
                            if (IsBeginGroup(field))
                            {
                                this.StartSelection(field);
                            }
                        }
                        else if (IsEndGroup(field))
                        {
                            this.EndSelection(field);
                            if (this.SendGroupFound != null)
                            {
                                this.SendGroupFound(this.m_rowsEnum);
                            }
                        }
                    }
                }
            }

            private void ClearSelection()
            {
                this.m_groupSelection = null;
                this.m_rowSelection = null;
                this.m_beginGroupField = null;
                this.m_endGroupField = null;
            }

            private void EndSelection(WMergeField field)
            {
                this.m_endGroupField = field;
                WTextBody ownerTextBody = field.OwnerParagraph.OwnerTextBody;
                this.m_selBodyItemsCnt = (this.m_bodyItemIndex - this.m_bodyItemStartIndex) + 1;
                if (ownerTextBody == this.m_groupTextBody)
                {
                    this.m_groupSelection = new TextBodySelection(ownerTextBody, this.m_bodyItemStartIndex, this.m_bodyItemIndex, this.m_paragraphItemStartIndex, this.m_paragraphItemIndex);
                }
                else
                {
                    //fixed internal prop by artem1
                    //if (((ownerTextBody.EntityType != EntityType.TableCell) || (this.m_groupTextBody.EntityType != EntityType.TableCell)) || ((this.m_groupTextBody.Owner as WTableRow).OwnerTable != (ownerTextBody.Owner as WTableRow).OwnerTable))
                    if (((ownerTextBody.EntityType != EntityType.TableCell) || 
                        (this.m_groupTextBody.EntityType != EntityType.TableCell)) || 
                        (((this.m_groupTextBody.Owner as WTableRow).Owner as WTable) != ((ownerTextBody.Owner as WTableRow).Owner as WTable)))
                    {
                        throw new MailMergeException();
                    }
                    this.UpdateEndSelection(ownerTextBody as WTableCell);
                    this.m_rowSelection = new TableRowSelection(ownerTextBody.Owner.Owner as WTable, this.m_startRowIndex, this.m_rowIndex);
                }
            }

            private void FindInBodyItems(BodyItemCollection bodyItems)
            {
                int itemEndIndex = 0;
                int count = bodyItems.Count;
                while (itemEndIndex < count)
                {
                    WParagraph paragraph;
                    int num3;
                    ParagraphItem item2;

                    //fixed can't convert by artem1
                    //TextBodyItem item = bodyItems[itemEndIndex];
                    TextBodyItem item = (TextBodyItem)bodyItems[itemEndIndex];
                    //end fix

                    this.m_bodyItemIndex = itemEndIndex;
                    switch (item.EntityType)
                    {
                        case EntityType.Paragraph:
                            paragraph = (WParagraph)item;
                            num3 = 0;
                            goto Label_00EF;

                        case EntityType.Table:
                            {
                                WTable table = (WTable)item;
                                this.FindInTable(table, 0, table.Rows.Count - 1);
                                goto Label_012A;
                            }
                        default:
                            throw new Exception();
                    }
                Label_004A:
                    item2 = paragraph.Items[num3];
                    if ((item2 is BookmarkStart) || (item2 is BookmarkEnd))
                    {
                        paragraph.Items.RemoveAt(num3);
                        num3--;
                    }
                    else
                    {
                        this.m_paragraphItemIndex = num3;
                        if (item2 is WTextBox)
                        {
                            this.m_bodyItemIndex = 0;

                            //fixed internal prop by artem1
                            //this.FindInBodyItems((item2 as WTextBox).TextBoxBody.Items);
                            this.FindInBodyItems((BodyItemCollection)(item2 as WTextBox).TextBoxBody.ChildEntities);
                            //end fix

                            this.m_bodyItemIndex = itemEndIndex;
                        }
                        else
                        {
                            this.CheckItem(item2);
                        }
                        if (this.IsGroupFound)
                        {
                            if (this.m_groupSelection == null)
                            {
                                goto Label_012A;
                            }
                            itemEndIndex = this.m_groupSelection.ItemEndIndex;
                            count = bodyItems.Count;
                            this.ClearSelection();
                        }
                    }
                    num3++;
                Label_00EF:
                    if (num3 < paragraph.Items.Count)
                    {
                        goto Label_004A;
                    }
                Label_012A:
                    itemEndIndex++;
                }
            }

            private void FindInTable(WTable table, int startRow, int endRow)
            {
                int count = table.Rows.Count;
                for (int i = startRow; i <= endRow; i++)
                {
                    WTableRow row = table.Rows[i];
                    this.m_rowIndex = i;
                    int num3 = 0;
                    int num4 = row.Cells.Count;
                    while (num3 < num4)
                    {
                        WTableCell cell = row.Cells[num3];
                        
                        //fixed internal prop by artem1
                        //this.FindInBodyItems(cell.Items);
                        this.FindInBodyItems((BodyItemCollection)cell.ChildEntities);
                        //end fix

                        if (this.IsGroupFound)
                        {
                            endRow += table.Rows.Count - count;
                            i = this.m_rowSelection.StartRowIndex;
                            this.ClearSelection();
                            break;
                        }
                        num3++;
                    }
                }
            }

            private void InitProcess(IRowsEnumerator rowsEnum)
            {
                this.m_groupSelection = null;
                this.m_rowSelection = null;
                this.m_beginGroupField = null;
                this.m_endGroupField = null;
                this.m_bodyItemIndex = 0;
                this.m_bodyItemStartIndex = -1;
                this.m_paragraphItemIndex = -1;
                this.m_paragraphItemStartIndex = -1;
                this.m_rowIndex = -1;
                this.m_selBodyItemsCnt = -1;
                this.m_rowsEnum = rowsEnum;
                this.m_groupName = this.m_rowsEnum.TableName;
            }

            internal void ProcessGroups(WTextBody body, IRowsEnumerator rowsEnum)
            {
                this.InitProcess(rowsEnum);
                this.m_groupTextBody = this.m_body = body;

                //fixed internal prop by artem1
                //this.FindInBodyItems(this.m_body.Items);
                this.FindInBodyItems((BodyItemCollection)this.m_body.ChildEntities);
                //endfix
            }

            internal void ProcessGroups(WTable table, int startRow, int endRow, IRowsEnumerator rowsEnum)
            {
                this.InitProcess(rowsEnum);
                this.FindInTable(table, startRow, endRow);
            }

            private void StartSelection(WMergeField field)
            {
                this.m_beginGroupField = field;
                this.m_groupTextBody = field.OwnerParagraph.OwnerTextBody;
                this.m_bodyItemStartIndex = this.m_bodyItemIndex;
                this.m_paragraphItemStartIndex = this.m_paragraphItemIndex;
                this.m_startRowIndex = this.m_rowIndex;
            }

            private void UpdateEndSelection(WTableCell cell)
            {
                WTableRow ownerRow = cell.OwnerRow;
                bool flag = false;

                //fixed reflector strange behavior - IEnumerator as IDisposable
                /*using (var enumerator = ownerRow.Cells.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        WTableCell current = (WTableCell)enumerator.Current;
                        if (cell.CellFormat.VerticalMerge != CellMerge.None)
                        {
                            flag = true;
                            goto Label_004F;
                        }
                    }
                }*/
                var enumerator = ownerRow.Cells.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        WTableCell current = (WTableCell)enumerator.Current;
                        if (cell.CellFormat.VerticalMerge != CellMerge.None)
                        {
                            flag = true;
                            goto Label_004F;
                        }
                    }
                } finally
                {
                    var disposable = enumerator as IDisposable;
                    if (disposable != null) disposable.Dispose(); 
                }
                //end fix

            Label_004F:
                if (flag)
                {
                    goto Label_00BF;
                }
                return;
            Label_00AE:
                if (!flag)
                {
                    return;
                }
                this.m_rowIndex++;
            Label_00BF:
                if (ownerRow.NextSibling != null)
                {
                    ownerRow = ownerRow.NextSibling as WTableRow;
                    flag = false;

                    //fixed reflector strange behavior - IEnumerator as IDisposable
                    /*
                     using(IEnumerator enumerator2 = ownerRow.Cells.GetEnumerator())
                     {
                        while (enumerator2.MoveNext())
                        {
                            WTableCell cell2 = (WTableCell)enumerator2.Current;
                            if (cell.CellFormat.VerticalMerge != CellMerge.None)
                            {
                                flag = true;
                                goto Label_00AE;
                            }
                        }
                    }
                     */
                    var enumerator2 = ownerRow.Cells.GetEnumerator();
                    try
                    {
                        while (enumerator2.MoveNext())
                        {
                            WTableCell cell2 = (WTableCell)enumerator2.Current;
                            if (cell.CellFormat.VerticalMerge != CellMerge.None)
                            {
                                flag = true;
                                goto Label_00AE;
                            }
                        }
                    }
                    finally
                    {
                        var disposable2 = enumerator2 as IDisposable;
                        if (disposable2 != null) disposable2.Dispose();
                    }
                    //end fix
                    goto Label_00AE;
                }
            }

            // Properties
            internal WMergeField BeginGroupField
            {
                get
                {
                    return this.m_beginGroupField;
                }
            }

            internal int BodyItemIndex
            {
                get
                {
                    return this.m_bodyItemIndex;
                }
                set
                {
                    this.m_bodyItemIndex = value;
                }
            }

            internal WMergeField EndGroupField
            {
                get
                {
                    return this.m_endGroupField;
                }
                set
                {
                    this.m_endGroupField = value;
                }
            }

            internal string GroupName
            {
                get
                {
                    return this.m_groupName;
                }
            }

            internal TextBodySelection GroupSelection
            {
                get
                {
                    return this.m_groupSelection;
                }
            }

            internal bool IsGroupFound
            {
                get
                {
                    return (this.m_endGroupField != null);
                }
            }

            internal TableRowSelection RowSelection
            {
                get
                {
                    return this.m_rowSelection;
                }
            }

            internal int SelectedBodyItemsCount
            {
                get
                {
                    return this.m_selBodyItemsCnt;
                }
            }

            // Nested Types
            internal delegate void GroupFound(IRowsEnumerator rowsEnum);
        }



        internal class TableRowSelection
        {
            // Fields
            internal int EndRowIndex;
            internal int StartRowIndex;
            internal WTable Table;

            // Methods
            internal TableRowSelection(WTable table, int startRowIndex, int endRowIndex)
            {
                this.Table = table;
                this.StartRowIndex = startRowIndex;
                this.EndRowIndex = endRowIndex;
                this.ValidateIndexes();
            }

            private void ValidateIndexes()
            {
                if ((this.StartRowIndex < 0) || (this.StartRowIndex >= this.Table.Rows.Count))
                {
                    throw new ArgumentOutOfRangeException("StartRowIndex");
                }
                if ((this.EndRowIndex < 0) || (this.EndRowIndex >= this.Table.Rows.Count))
                {
                    throw new ArgumentOutOfRangeException("EndRowIndex");
                }
            }
        }
    }
}
// ReSharper restore UnassignedField.Local
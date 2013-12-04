using System;
using System.Globalization;
using System.Threading;
using System.Xml;
using StringPair = System.Tuple<string, string>;


namespace AGO.Reporting.Common
{
    /// <summary>
    /// Базовый класс-генератор данных для отчетов со всеми необходимыми сервисами
    /// </summary>
    public abstract class BaseReportDataGenerator : IReportDataGenerator, IProgressTracker
    {
        private XmlDocument _doc;
        private static readonly DateTime minDate = new DateTime(1753, 1, 1);
        private PercentTicker ticker;
    	private CancellationToken cancellationToken;

        /// <summary>
        /// Результирующий xml-документ - результат работы с генератором.
        /// </summary>
        public XmlDocument Document
        {
            get { return _doc; }
        }

        #region Форматирование

        /// <summary>
        /// Переводит сумму в строку принятым в старом TIN-е способом (для отображения сумм
        /// в тексте документов)
        /// 1 458.22 р. - 1458 (Одна тысяча четыреста пятьдесят восемь) рублей 22 копейки
        /// </summary>
        /// <param name="value">Денежное значение</param>
        /// <param name="withCapitalLetter">Флаг для выбора регистра вывода текстового числа</param>
        /// <returns>Значение в виде строки</returns>
        protected static string ToDocMoneyString(decimal value, bool withCapitalLetter)
        {
            decimal rubValue = Math.Truncate(value);
            decimal copValue = Math.Truncate((value - rubValue) * 100);
            string sRubValue;
            string sCopValue; //Нужно только чтобы выяснить правильное написание копеек
            string rubText = "рублей";
            string copText = "копеек";
            GrammaticalForm f = CurrencyConverter.NumberToString(Convert.ToInt64(rubValue), out sRubValue, GrammaticalTriadForm.Full, GrammaticalGender.Male);
            switch (f)
            {
                case GrammaticalForm.First:
                    rubText = "рубль";
                    break;
                case GrammaticalForm.Second:
                    rubText = "рубля";
                    break;
                case GrammaticalForm.Third:
                    rubText = "рублей";
                    break;
            }
            //Нужно только чтобы выяснить правильное написание копеек
            f = CurrencyConverter.NumberToString(Convert.ToInt64(copValue), out sCopValue, GrammaticalTriadForm.Full, GrammaticalGender.Female);
            switch (f)
            {
                case GrammaticalForm.First:
                    copText = "копейка";
                    break;
                case GrammaticalForm.Second:
                    copText = "копейки";
                    break;
                case GrammaticalForm.Third:
                    copText = "копеек";
                    break;
            }

            sRubValue = sRubValue.Trim();
            if (withCapitalLetter && !String.IsNullOrEmpty(sRubValue))
            {
                sRubValue = sRubValue[0].ToString(CultureInfo.CurrentCulture).ToUpper() + sRubValue.Substring(1);
            }
            return string.Format("{0:N0} ({1}) {2} {3:N0} {4}", rubValue, sRubValue, rubText, copValue, copText);
        }

        protected static bool IsNotEmptyDate(DateTime date)
        {
            return date > minDate;
        }

        #endregion

        #region Создание документа

        /// <summary>
        /// Создает основу (каркас) xml-документа для хранения данных отчета.
        /// </summary>
        /// <returns></returns>
        internal protected XmlDocument MakeDocument()
        {
            _doc = new XmlDocument();
            //Решено отключить схему, т.к. валидация замедляет работу
            //XmlNode root = _doc.CreateElement(null, "reportData", "http://agosystems.com/core/reporting/ReportDataSchema.xsd");
            XmlNode root = _doc.CreateElement("reportData");
            XmlDeclaration decl = _doc.CreateXmlDeclaration("1.0", "UTF8", "yes");
            _doc.AppendChild(decl);
            _doc.AppendChild(root);

            return _doc;
        }

        /// <summary>
        /// Имя результирующего файла отчета (необязательно).
        /// </summary>
        protected string ReportName
        {
            get { return _doc.DocumentElement.GetAttribute("reportName"); }
            set { _doc.DocumentElement.SetAttribute("reportName", value); }
        }

        #endregion

        #region Диапазон

        /// <summary>
        /// Создает диапазон данных.
        /// </summary>
        /// <param name="rangeName">Имя диапазона.</param>
        /// <param name="sheetName">Имя листа в отчете, на котором будет распологаться диапазон.</param>
        /// <returns>Xml-элемент диапазона данных.</returns>
        protected internal XmlElement MakeRange(string rangeName, string sheetName)
        {
            XmlElement rangeElement = _doc.CreateElement("range");
            XmlAttribute nameAttribute = _doc.CreateAttribute("name");
            nameAttribute.Value = rangeName;
            rangeElement.Attributes.Append(nameAttribute);
            XmlAttribute sheetNameAttribute = _doc.CreateAttribute("sheetName");
            sheetNameAttribute.Value = sheetName;
            rangeElement.Attributes.Append(sheetNameAttribute);

            _doc.DocumentElement.AppendChild(rangeElement);

            return rangeElement;
        }

        #endregion

        #region Строка в диапазоне

        /// <summary>
        /// Элемент данных (строка) в диапазоне данных.
        /// </summary>
        /// <param name="range">Диапазон данных.</param>
        /// <returns>Xml-элемент данных (строка).</returns>
        protected internal XmlElement MakeItem(XmlElement range)
        {
            XmlElement item = _doc.CreateElement("item");
            range.AppendChild(item);

            return item;
        }

        /// <summary>
        /// Элемент данных (строка) в диапазоне данных.
        /// </summary>
        /// <param name="range">Диапазон данных.</param>
        /// <returns>Xml-элемент данных (строка).</returns>
        internal XmlElement InsertFirstItem(XmlElement range)
        {
            XmlElement item = _doc.CreateElement("item");
            range.InsertBefore(item, range.FirstChild);

            return item;
        }

        /// <summary>
        /// Создает вложенный диапазон данных (вложенные группы).
        /// <remarks>Текущая реализация поддерживает только один уровень вложенности (пока).</remarks>
        /// </summary>
        /// <param name="item">Строка в диапазоне, для которой надо создать вложенный диапазон.</param>
        /// <param name="subRangeName">Имя вложенного диапазона.</param>
        /// <returns>Xml-элемент вложенного диапазона данных.</returns>
        protected internal XmlElement MakeSubRange(XmlElement item, string subRangeName)
        {
            XmlElement subRangeElement = _doc.CreateElement("subRange");
            XmlAttribute nameAttribute = _doc.CreateAttribute("name");
            nameAttribute.Value = subRangeName;
            subRangeElement.Attributes.Append(nameAttribute);
            item.AppendChild(subRangeElement);

            return subRangeElement;
        }

        #endregion

        #region Значения

        /// <summary>
        /// Создает ячейку данных в элементе данных (строке).
        /// </summary>
        /// <param name="item">Элемент данных (строка).</param>
        /// <param name="valueName">Имя маркера.</param>
        /// <param name="value">Значение.</param>
        /// <param name="typify"><b>true</b>, если данные, переданные в параметре <paramref name="value"/>
        /// надо пытаться привести к какому-то точному типу (целое число, дробное число, дата). Это директива
        /// для генератора файла отчетов, генератору данных этот параметр не нужен.</param>
        /// <param name="group">Имя группы ячеек данных для их последующей группировки (объединения ячеек) в отчете.
        /// Может быть пустым.</param>
        /// <returns>Xml-элемент ячейки данных.</returns>
        protected internal XmlElement MakeValue(XmlElement item, string valueName, string value, bool typify, string group)
        {
            XmlElement valueElement = _doc.CreateElement("value");

            XmlAttribute nameAttribute = _doc.CreateAttribute("name");
            nameAttribute.Value = valueName;
            valueElement.Attributes.Append(nameAttribute);
            XmlAttribute typifyAttribute = _doc.CreateAttribute("typify");
            typifyAttribute.Value = typify.ToString();
            valueElement.Attributes.Append(typifyAttribute);
            if (!string.IsNullOrEmpty(group))
            {
                XmlAttribute groupAttribute = _doc.CreateAttribute("group");
                groupAttribute.Value = group;
                valueElement.Attributes.Append(groupAttribute);
            }

            valueElement.AppendChild(_doc.CreateCDataSection(value));

            item.AppendChild(valueElement);

            return valueElement;
        }

        /// <summary>
        /// Создает ячейку данных в элементе данных (строке). Без типизации.
        /// </summary>
        /// <param name="item">Элемент данных (строка).</param>
        /// <param name="valueName">Имя маркера.</param>
        /// <param name="value">Значение.</param>
        /// <param name="group">Имя группы ячеек данных для их последующей группировки (объединения ячеек) в отчете.
        /// Может быть пустым.</param>
        /// <returns>Xml-элемент ячейки данных.</returns>
        protected internal XmlElement MakeValue(XmlElement item, string valueName, string value, string group)
        {
            return MakeValue(item, valueName, value, false, group);
        }

        /// <summary>
        /// Создает ячейку данных в элементе данных (строке).
        /// </summary>
        /// <param name="item">Элемент данных (строка).</param>
        /// <param name="valueName">Имя маркера.</param>
        /// <param name="value">Значение.</param>
        /// <param name="typify"><b>true</b>, если данные, переданные в параметре <paramref name="value"/>
        /// надо пытаться привести к какому-то точному типу (целое число, дробное число, дата). Это директива
        /// для генератора файла отчетов, генератору данных этот параметр не нужен.</param>
        /// Может быть пустым.
        /// <returns>Xml-элемент ячейки данных.</returns>
        protected internal XmlElement MakeValue(XmlElement item, string valueName, string value, bool typify)
        {
            return MakeValue(item, valueName, value, typify, string.Empty);
        }

        /// <summary>
        /// Создает ячейку данных в элементе данных (строке).
        /// </summary>
        /// <param name="item">Элемент данных (строка).</param>
        /// <param name="valueName">Имя маркера.</param>
        /// <param name="value">Значение.</param>
        /// <returns>Xml-элемент ячейки данных.</returns>
        protected internal XmlElement MakeValue(XmlElement item, string valueName, string value)
        {
            return MakeValue(item, valueName, value, true, null);
        }

        protected internal XmlElement MakeSimpleValueRange(string rangeName, string sheetName, string marker, string value)
        {
            return MakeSimpleValueRange(rangeName, sheetName, new StringPair(marker, value));
        }

        protected internal XmlElement MakeSimpleValueRange(string rangeName, string sheetName, StringPair valueData)
        {
            return MakeSimpleValueRange(rangeName, sheetName, new[] { valueData });
        }

        protected internal XmlElement MakeSimpleValueRange(string rangeName, string sheetName, params StringPair[] valueData)
        {
            XmlElement range = MakeRange(rangeName, sheetName);
            XmlElement item = MakeItem(range);

            foreach (StringPair pair in valueData)
            {
                MakeValue(item, pair.Item1, pair.Item2);
            }
            return range;
        }

        #region Специальные значения (файлы)

        protected XmlElement MakeSimpleFileValueRange(string rangeName, string sheetName, string marker, string file)
        {
            var range = MakeRange(rangeName, sheetName);
            var item = MakeItem(range);
            return MakeFileItemValue(item, marker, file);
        }

        /// <summary>
        /// Создает ячейку данных в элементе (строке). В качестве значения используется
        /// полное имя файла <paramref name="file"/>, хранящегося в нашем хранилище.
        /// </summary>
        /// <param name="item">Элемент данных (строка).</param>
        /// <param name="valueName">Имя маркера.</param>
        /// <param name="file">Путь к файлу.</param>
        /// <returns>Xml-элемент ячейки данных.</returns>
        protected XmlElement MakeFileItemValue(XmlElement item, string valueName, string file)
        {
            return MakeValue(item, valueName, file);
        }

        #endregion

        #endregion

        #region IProgressTracker Members

        protected void InitTicker(int ticks)
        {
            ticker = new PercentTicker(ticks);
			ticker.Changed += (s, e) => OnProgressChanged();
        }

        internal void InitTicker(PercentTicker ticker)
        {
            this.ticker = ticker;
        }

        protected virtual void OnProgressChanged()
        {
            if (ProgressChanged != null) ProgressChanged(this, EventArgs.Empty);
			if (cancellationToken != null)
				cancellationToken.ThrowIfCancellationRequested();
        }

        public PercentTicker Ticker
        {
            get { return ticker; }
        }

        public int PercentCompleted
        {
            get { return ticker != null ? ticker.PercentCompleted : 0; }
        }

        public event EventHandler ProgressChanged;

        #endregion

        #region IReportDataGenerator Members

        public virtual XmlDocument GetReportData(object parameters, CancellationToken token)
        {
        	cancellationToken = token;
        	MakeDocument();
			FillReportData(parameters);
        	return Document;
        }

    	protected abstract void FillReportData(object parameters);

    	#endregion
    }

    public static class ReportingXmlElementExtensions
    {

        /// <summary>
        /// Добавляет форматирование к элементу данных, заданному тегом <b>value</b>.
        /// Элемент данных должен быть получен вызовом метода <see cref="BaseReportDataGenerator.MakeValue(XmlElement,string,string)"/>
        /// или любой из его версий.
        /// Возвращается тот же элемент данных (для method chaining).
        /// Этот метод рассчитывает, что значение будет расположено в ячейке таблицы.
        /// </summary>
        /// <param name="cell">Элемент данных</param>
        /// <param name="fmt">Формат</param>
        /// <returns>Элемент данных.</returns>
        public static XmlElement AddFormat(this XmlElement cell, ReportCellFormat fmt)
        {
            AddFormatAttribute(cell, fmt.ToString());
            return cell;
        }

        /// <summary>
        /// Добавляет форматирование к элементу данных, заданному тегом <b>value</b>.
        /// Элемент данных должен быть получен вызовом метода <see cref="BaseReportDataGenerator.MakeValue(XmlElement,string,string)"/>
        /// или любой из его версий.
        /// Возвращается тот же элемент данных (для method chaining).
        /// Этот метод рассчитывает, что значение будет расположено просто в тексте параграфа.
        /// </summary>
        /// <param name="cell">Элемент данных</param>
        /// <param name="fmt">Формат</param>
        /// <returns>Элемент данных.</returns>
        public static XmlElement AddFormat(this XmlElement cell, ReportFieldFormat fmt)
        {
            AddFormatAttribute(cell, fmt.ToString());
            return cell;
        }

        /// <summary>
        /// Добавляет форматирование группы элементов данных к элементу, диапазона, заданному тегом <b>range</b>.
        /// Элемент должен быть получен вызовом метода <see cref="BaseReportDataGenerator.MakeRange"/>.
        /// Возвращается тот же элемент диапазона данных (для method chaining).
        /// </summary>
        /// <param name="rangeOrSubRange">Элемент диапазона данных (или вложенного диапазона).</param>
        /// <param name="fmt">Формат</param>
        /// <returns>Элемент диапазона (вложенного диапазона) данных</returns>
        public static XmlElement AddGroupFormat(this XmlElement rangeOrSubRange, GroupCellFormat fmt)
        {
            var doc = rangeOrSubRange.OwnerDocument;
            var group = doc.CreateElement("group");
            var nameAttribute = doc.CreateAttribute("name");
            nameAttribute.Value = fmt.Group;
            group.Attributes.Append(nameAttribute);
            AddFormatAttribute(group, fmt.ToString());
            rangeOrSubRange.AppendChild(group);
            return rangeOrSubRange;
        }

        /// <summary>
        /// Добавляет форматирование страницы в отчета.
        /// Должен применяться к <see cref="XmlDocument"/>, полученному вызовом метода <see cref="BaseReportDataGenerator.MakeDocument()"/>.
        /// </summary>
        /// <param name="doc">Документ с данными отчета</param>
        /// <param name="pst">Параметры страницы</param>
        /// <returns>Документ с данными отчета</returns>
        public static XmlDocument AddReportFormat(this XmlDocument doc, ReportPageSetup pst)
        {
            var pageSetupAttribute = doc.CreateAttribute("pageSetup");
            pageSetupAttribute.Value = pst.ToString();
            doc.DocumentElement.Attributes.Append(pageSetupAttribute);
            return doc;
        }

        private static void AddFormatAttribute(XmlElement to, string fmt)
        {
            var doc = to.OwnerDocument;
            var formatAttribute = doc.CreateAttribute("format");
            formatAttribute.Value = fmt;
            to.Attributes.Append(formatAttribute);
        }
    }
}
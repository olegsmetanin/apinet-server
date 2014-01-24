using System;
using System.Threading;
using AGO.Reporting.Common;

namespace AGO.Reporting.Service.ReportGenerators
{
    /// <summary>
    /// Базовый генератор отчетов. Содержит общую функциональность.
    /// </summary>
    public abstract class BaseReportGenerator: IProgressTracker
    {
        protected const string XPATH_RANGES = "range";
        protected const string XPATH_SHEETNAME = "@sheetName";
        protected const string XPATH_NAME = "@name";
        protected const string XPATH_TYPIFY = "@typify";
        protected const string XPATH_GROUP = "@group";
        protected const string XPATH_ITEMS = "item";
        protected const string XPATH_VALUES = "value";

        private const string MARKER_START_SYMBOLS = "{$";
        private const string MARKER_END_SYMBOLS = "$}";

        protected virtual string MarkerStartSymbol
        {
            get { return MARKER_START_SYMBOLS; }
        }

        protected virtual string MarkerEndSymbol
        {
            get { return MARKER_END_SYMBOLS; }
        }

        /// <summary>
        /// Добавляет к имени маркера спец.символы
        /// </summary>
        /// <param name="markerName">Имя маркера без спецсимволов</param>
        /// <returns>Полноценное имя маркера, используемое в шаблонах отчетов</returns>
        protected string CompleteMarker(string markerName)
        {
            if (markerName.StartsWith(MarkerStartSymbol) && markerName.EndsWith(MarkerEndSymbol))
                return markerName;
            return string.Concat(MarkerStartSymbol, markerName, MarkerEndSymbol);
        }

        #region Progress tracking
        private PercentTicker ticker;
    	protected CancellationToken CancellationToken;

    	protected void InitTicker(int ticks)
        {
            ticker = new PercentTicker(ticks);
            ticker.Changed += OnChanged;
        }

        protected PercentTicker Ticker
        {
            get { return ticker; }
        }

        public byte PercentCompleted
        {
            get { return ticker != null ? ticker.PercentCompleted : byte.MinValue; }
        }

        protected virtual void OnChanged(object sender, EventArgs e)
        {
            if (ProgressChanged != null) 
				ProgressChanged(this, EventArgs.Empty);

			CancellationToken.ThrowIfCancellationRequested();
        }

        public event EventHandler ProgressChanged;
        #endregion
    }
}
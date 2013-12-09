using System;

namespace AGO.Reporting.Common
{
    /// <summary>
    /// Служит для упрощения подсчета хода выполнения различных итерационных процессов в %
    /// </summary>
    /// <example>
    ///     <code>
    ///     class Program
    ///    {
    ///        static void Main(string[] args)
    ///        {
    ///            int[] ints = { 1, 5, 8, 7};
    ///
    ///            PercentTicker ticker = new PercentTicker(ints.Length);
    ///            ticker.PercentCompletedChanged += ticker_PercentCompletedChanged;
    ///
    ///            foreach (int i in ints)
    ///            {
    ///                ticker.AddTick();
    ///            }
    /// 
    ///            ticker.Ticks += 5;
    ///            for(int i = 0; i &lt; 5; i++) {
    ///                ticker.AddTick();
    ///            }
    ///        }
    ///
    ///        static void ticker_PercentCompletedChanged(object sender, EventArgs e)
    ///        {
    ///            PercentTicker ticker = (PercentTicker) sender;
    ///            Console.WriteLine("{0}%", ticker.PercentCompleted);
    ///        }
    ///    }
    ///     </code>
    /// </example>
    /// <remarks>В будущем возмжно понадобиться модификация, которая позволяет добавлять кол-во итераций
    /// и таким образом перерасчитывать процент выполнения. Необходимо для процессов, кол-во итераций 
    /// которых сразу задать невозможно.</remarks>
    public sealed class PercentTicker
    {
        private int ticks;
        private int completedTicks;
        private double percentInTick;

        /// <summary>
        /// Инициализирует класс кол-ом тиков, которые он должен "оттикать"
        /// </summary>
        /// <param name="ticks">Кол-во тиков</param>
        public PercentTicker(int ticks)
        {
            Ticks = ticks;
            completedTicks = 0;
        }

        /// <summary>
        /// Общее кол-во тиков, которые класс должен оттикать
        /// </summary>
        public int Ticks
        {
            get { return ticks; }
            set
            {
                ticks = value;
                percentInTick = 100.0 / ticks;
            }
        }

        /// <summary>
        /// Процентов выполнено (вычисляется от кол-ва пройденных тиков и их общего кол-ва, т.к.
        /// общее кол-во может изменяться в процессе работы
        /// </summary>
        public byte PercentCompleted
        {
            get { return Convert.ToByte(Math.Min(Math.Round(completedTicks * percentInTick), 100)); }
        }

        /// <summary>
        /// Добавляет следующий шаг
        /// </summary>
        public void AddTick()
        {
            completedTicks++;
            //Защита от некорректно рассчитанного кол-ва тиков. Если кол-во выполненных
            //превысит кол-во всех тиков, то процент пойдет больше 100. Поэтому в случае превышения
            //корректируем общее кол-во тиков, причем именно через свойство, т.к. там происходит
            //перерасчет тиков на процент.
            if (ticks < completedTicks) Ticks = completedTicks;
            OnPercentCompletedChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Добавляет следующий шаг
        /// </summary>
        public void AddTicks(int cnt) {

            int i;
			for (i = 0; i < cnt; i++)
                AddTick();
        }

        /// <summary>
        /// Срабатывает при изменении процента выполненных итераций
        /// </summary>
        public event EventHandler Changed;

        private void OnPercentCompletedChanged(EventArgs e)
        {
            if (Changed != null)
            {
                Changed(this, e);
            }
        }
    }
}
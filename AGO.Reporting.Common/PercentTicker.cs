using System;

namespace AGO.Reporting.Common
{
    /// <summary>
    /// ������ ��� ��������� �������� ���� ���������� ��������� ������������ ��������� � %
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
    /// <remarks>� ������� ������� ������������ �����������, ������� ��������� ��������� ���-�� ��������
    /// � ����� ������� ��������������� ������� ����������. ���������� ��� ���������, ���-�� �������� 
    /// ������� ����� ������ ����������.</remarks>
    public sealed class PercentTicker
    {
        private int ticks;
        private int completedTicks;
        private double percentInTick;

        /// <summary>
        /// �������������� ����� ���-�� �����, ������� �� ������ "��������"
        /// </summary>
        /// <param name="ticks">���-�� �����</param>
        public PercentTicker(int ticks)
        {
            Ticks = ticks;
            completedTicks = 0;
        }

        /// <summary>
        /// ����� ���-�� �����, ������� ����� ������ ��������
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
        /// ��������� ��������� (����������� �� ���-�� ���������� ����� � �� ������ ���-��, �.�.
        /// ����� ���-�� ����� ���������� � �������� ������
        /// </summary>
        public byte PercentCompleted
        {
            get { return Convert.ToByte(Math.Min(Math.Round(completedTicks * percentInTick), 100)); }
        }

        /// <summary>
        /// ��������� ��������� ���
        /// </summary>
        public void AddTick()
        {
            completedTicks++;
            //������ �� ����������� ������������� ���-�� �����. ���� ���-�� �����������
            //�������� ���-�� ���� �����, �� ������� ������ ������ 100. ������� � ������ ����������
            //������������ ����� ���-�� �����, ������ ������ ����� ��������, �.�. ��� ����������
            //���������� ����� �� �������.
            if (ticks < completedTicks) Ticks = completedTicks;
            OnPercentCompletedChanged(EventArgs.Empty);
        }

        /// <summary>
        /// ��������� ��������� ���
        /// </summary>
        public void AddTicks(int cnt) {

            int i;
			for (i = 0; i < cnt; i++)
                AddTick();
        }

        /// <summary>
        /// ����������� ��� ��������� �������� ����������� ��������
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
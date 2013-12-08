using System;
using System.Threading;

namespace AGO.Reporting.Service
{
	public sealed class SequentialTimer: IDisposable
	{
		private readonly Action action;
		private Timer timer;
		private readonly object lc;

		public SequentialTimer(Action action, int interval = Timeout.Infinite)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			this.action = action;
			Interval = interval;
			lc = new object();
			
			timer = new Timer(o => this.action(), null, Timeout.Infinite, Timeout.Infinite);
		}

		public int Interval { get; set; }

		public bool Run(int? when = null)
		{
			lock (lc)
			{
				if (timer == null)
					throw new ObjectDisposedException("SequentialTimer");

				var nextrun = when.HasValue ? when.Value : Interval;
				return timer.Change(nextrun, Timeout.Infinite);
			}
		}

		public void Stop()
		{
			lock (lc)
			{
				if (timer == null) return;

				timer.Dispose();
				timer = null;
			}
		}

		public void Dispose()
		{
			Stop();
		}
	}
}
using System;
using System.Linq;
using System.Threading;
using AGO.Core.Notification;
using AGO.Reporting.Common;
using AGO.WorkQueue;
using Common.Logging;
using Newtonsoft.Json;

namespace AGO.Core.Watchers
{
	/// <summary>
	/// Наблюдает за событиями в системе, распространяемыми через <see cref="INotificationService"/> и рассылкает клиентам
	/// информацию о позиции их задач в общей очереди <see cref="IWorkQueue"/>.
	/// </summary>
	public class WorkQueueWatchService: IDisposable
	{
		private readonly INotificationService notifications;
		private readonly IWorkQueue queue;
		private readonly Timer pullTimer;
		private readonly int throttle;
		private long changes;
		private DateTime lastRun;

		//Ioc need this constructor
		public WorkQueueWatchService(INotificationService ns, IWorkQueue wq)
			: this(ns, wq, 2000)
		{
			
		}

		internal WorkQueueWatchService(INotificationService ns, IWorkQueue wq, int throttleDelay)
		{
			if (ns == null)
				throw new ArgumentNullException("ns");
			if (wq == null)
				throw new ArgumentNullException("wq");

			notifications = ns;
			queue = wq;
			throttle = Math.Max(0, throttleDelay);
			lastRun = DateTime.MinValue;
			pullTimer = new Timer(notused => Process(), null, Timeout.Infinite, Timeout.Infinite);

			ns.SubscribeToReportChanged(OnReportChange);
			Process();
		}
		
		private void OnReportChange(string type, string login, object dto)
		{
			//Only interesting in events, that change position in work queue
			if (type != ReportEvents.CREATED && type != ReportEvents.RUNNED && type != ReportEvents.CANCELED) return;
			if (1 == Interlocked.Increment(ref changes))
			{
				//changes was 0 before increment - run notification
				//if Process only completed and next we catch event - may be time delay less than throttle
				//so, need run timer with appropriate delay
				var msFromLastRun = Convert.ToInt64(DateTime.Now.Subtract(lastRun).TotalMilliseconds);
				var dueDate = Math.Max(0, throttle - msFromLastRun);
					//msFromLastRun > throttle ? 0 : throttle - msFromLastRun;
				pullTimer.Change(dueDate, Timeout.Infinite);
			}
		}

		private void Process()
		{
			//cache changes count at start of processing
			var prevChanges = Interlocked.CompareExchange(ref changes, 0, 0); //simple noop read operation, not needed to volatile changes


			try
			{
				var snapshot = queue.Snapshot();
				foreach (var key in snapshot.Keys)
				{
					var user = key;
					var userTasks = snapshot[key];
					var positions = userTasks
						.Keys.SelectMany(project => 
							userTasks[project].Select(projqi => 
								new ReportQueuePosition
								{
									Project = project,
									TaskId = projqi.TaskId,
									Position = projqi.OrderInQueue.GetValueOrDefault()
								}))
						.ToArray();
					notifications.EmitWorkQueueChanged(user, positions);
				}
			}
			catch (Exception ex)
			{
				LogManager.GetLogger(GetType()).Error("Error when process work queue snapshot", ex);
			}


			if (prevChanges < Interlocked.CompareExchange(ref changes, 0, 0))
			{
				//while notifications was processed there new events catched, 
				//plan for new run after throttling period
				pullTimer.Change(throttle, Timeout.Infinite);
			}
			else
			{
				//while notification was processed, no new events catched
				//reset counter and wait for next event, that set counter to 1 and start timer again (see OnReportChange)
				lastRun = DateTime.Now;
				Interlocked.Exchange(ref changes, 0);
			}
		}

		public void Dispose()
		{
			pullTimer.Dispose();
		}

		public sealed class ReportQueuePosition
		{
			[JsonProperty("project")]
			public string Project { get; set; }

			[JsonProperty("taskId")]
			public Guid TaskId { get; set; }

			[JsonProperty("position")]
			public int Position { get; set; }
		}
	}
}
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AGO.Core.Notification;
using AGO.Core.Watchers;
using AGO.Reporting.Common;
using AGO.WorkQueue;
using NSubstitute;
using NUnit.Framework;

namespace AGO.Core.Tests
{
	[TestFixture]
	public class WorkQueueWatcherTest
	{		
		
		private static Dictionary<string, IDictionary<string, QueueItem[]>> MakeEmptySnapshot()
		{
			return new Dictionary<string, IDictionary<string, QueueItem[]>>();
		}

		[Test]
		public void OnStartWatcherSubscribeToReportChanges()
		{
			var wq = Substitute.For<IWorkQueue>();
			var ns = Substitute.For<INotificationService>();

// ReSharper disable once UnusedVariable
			var watcher = new WorkQueueWatchService(ns, wq);

			ns.Received().SubscribeToReportChanged(Arg.Any<Action<string, string, object>>());
		}

		[Test]
		public void OnStartWatcherQueryWorkQueueForState()
		{
			var wq = Substitute.For<IWorkQueue>();
			wq.Snapshot().Returns(MakeEmptySnapshot());
			var ns = Substitute.For<INotificationService>();

// ReSharper disable once UnusedVariable
			var watcher = new WorkQueueWatchService(ns, wq);

			wq.Received().Snapshot();
		}

		[Test]
		public void WatcherThrottleQuering()
		{
			var wq = Substitute.For<IWorkQueue>();
			var callsTime = new List<DateTime>();
			wq.Snapshot().Returns(call =>
			{
				callsTime.Add(DateTime.Now);
				return MakeEmptySnapshot();
			});
			Action<string, string, object> subscriber = null;
			var ns = Substitute.For<INotificationService>();
			ns.WhenForAnyArgs(x => x.SubscribeToReportChanged(null)).Do(x => subscriber = x.Arg<Action<string, string, object>>());

			const int throttleDelay = 50; //lower value may exceed system timer accuracy
			const double epsilon = throttleDelay*0.1;//10%
			const int eventsCount = 40;
// ReSharper disable once UnusedVariable
			var watcher = new WorkQueueWatchService(ns, wq, throttleDelay);
			Task[] tasks = Enumerable.Range(0, eventsCount)
					.Select(i => Task.Run(() =>
					{
						Thread.Sleep((int) (throttleDelay * 0.9));
						subscriber(ReportEvents.CREATED, "user", null);
					}))
					.ToArray();
			Task.WaitAll(tasks);
			Thread.Sleep(throttleDelay * (eventsCount + 1));

			for (var i = 1; i < callsTime.Count; i++)
			{
				var a = callsTime[i - 1];
				var b = callsTime[i];
				var diff = b.Subtract(a);
				Debug.WriteLine(diff);
				Assert.That(diff.TotalMilliseconds, Is.GreaterThanOrEqualTo(throttleDelay - epsilon));
			}
		}

		[Test]
		public void NotificationsSentToLogins()
		{
			var wq = Substitute.For<IWorkQueue>();
			const string user1 = "user1";
			const string user2 = "user2";
			const string proj1 = "proj1";
			const string proj2 = "proj2";
			Func<string, string, int, QueueItem> makeqi = (p, uid, n) => new QueueItem("Report", Guid.NewGuid(), p, uid) {OrderInQueue = n};
			var testSnapshot = new Dictionary<string, IDictionary<string, QueueItem[]>>
			{
				{
					user1, new Dictionary<string, QueueItem[]>
					{
						{proj1, new[] {makeqi(proj1, user1, 3), makeqi(proj1, user1, 5)}},
						{proj2, new[] {makeqi(proj2, user1, 1)}}
					}
				},
				{
					user2, new Dictionary<string, QueueItem[]>
					{
						{proj1, new[] {makeqi(proj1, user2, 2)}},
						{proj2, new[] {makeqi(proj2, user2, 4), makeqi(proj2, user2, 7)}}
					}
				}

			};
			wq.Snapshot().Returns(testSnapshot);
			var ns = Substitute.For<INotificationService>();
			WorkQueueWatchService.ReportQueuePosition[] catched1 = null;
			WorkQueueWatchService.ReportQueuePosition[] catched2 = null;
			ns.When(x => x.EmitWorkQueueChanged(user1, Arg.Any<object>())).Do(call =>
			{
				catched1 = call.Arg<WorkQueueWatchService.ReportQueuePosition[]>();
			});
			ns.When(x => x.EmitWorkQueueChanged(user2, Arg.Any<object>())).Do(call =>
			{
				catched2 = call.Arg<WorkQueueWatchService.ReportQueuePosition[]>();
			});

			var watcher = new WorkQueueWatchService(ns, wq);

			wq.Received().Snapshot();
			
			ns.Received(1).EmitWorkQueueChanged(user1, Arg.Any<object>());
			Assert.IsNotNull(catched1);
			Assert.That(catched1.Length, Is.EqualTo(3));
			Assert.That(catched1, Has.Exactly(1).Matches<WorkQueueWatchService.ReportQueuePosition>(rqp => rqp.Project == proj1 && rqp.Position == 3));
			Assert.That(catched1, Has.Exactly(1).Matches<WorkQueueWatchService.ReportQueuePosition>(rqp => rqp.Project == proj1 && rqp.Position == 5));
			Assert.That(catched1, Has.Exactly(1).Matches<WorkQueueWatchService.ReportQueuePosition>(rqp => rqp.Project == proj2 && rqp.Position == 1));
			
			ns.Received(1).EmitWorkQueueChanged(user2, Arg.Any<object>());
			Assert.IsNotNull(catched2);
			Assert.That(catched2, Has.Exactly(1).Matches<WorkQueueWatchService.ReportQueuePosition>(rqp => rqp.Project == proj1 && rqp.Position == 2));
			Assert.That(catched2, Has.Exactly(1).Matches<WorkQueueWatchService.ReportQueuePosition>(rqp => rqp.Project == proj2 && rqp.Position == 4));
			Assert.That(catched2, Has.Exactly(1).Matches<WorkQueueWatchService.ReportQueuePosition>(rqp => rqp.Project == proj2 && rqp.Position == 7));

			watcher.Dispose();
		}
	}
}

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AGO.WorkQueue.Tests
{
	[TestFixture]
	public abstract class AbstractQueueParallelUsageTest
	{
		private IWorkQueue queue;
		private string user;
		private ConcurrentQueue<QueueItem> completed;

		protected abstract IWorkQueue CreateQueue();

		[SetUp]
		public void SetUp()
		{
			queue = CreateQueue();
			user = Guid.NewGuid().ToString();
			completed = new ConcurrentQueue<QueueItem>();
		}

		private void AddTask(Guid taskId, string project = "proj", int up = 0)
		{
			var task = new QueueItem("report", taskId, project, user)
			{
				PriorityType = up > 0 ? 1 : 0,
				UserPriority = up
			};
			queue.Add(task);
		}

		private void DoTask(string project = "proj", int workTime = 10)
		{
			var task = queue.Get(project);
			while(task != null)
			{
				completed.Enqueue(task);
				Thread.Sleep(workTime);

				task = queue.Get(project);
			}
		}

		private Task[] StartWorker(int count = 1, int workTime = 10)
		{
			var result = new Task[count];
			for (int i = 0; i < count; i++)
			{
				result[i] = Task.Run(() => DoTask(workTime: workTime));
			}
			return result;
		}

		[Test]
		public void SingleWorkerTest()
		{
			Action priority10TaskGenerator = () =>
			{
				for (var i = 0; i < 100; i++)
					AddTask(Guid.NewGuid(), up: 10);
			};

			Action priority5TaskGenerator = () =>
			{
				for (var i = 0; i < 100; i++)
					AddTask(Guid.NewGuid(), up: 5);
			};

			Action priority0TaskGenerator = () =>
			{
				for (var i = 0; i < 100; i++)
					AddTask(Guid.NewGuid());
			};

			var tasks = new[] {Task.Run(priority10TaskGenerator), Task.Run(priority5TaskGenerator), Task.Run(priority0TaskGenerator)};
			tasks = tasks.Concat(StartWorker()).ToArray();
			Task.WaitAll(tasks);

			Assert.AreEqual(300, completed.Count);
			for (var i = 0; i < 100; i++)
			{
				QueueItem qi;
				completed.TryDequeue(out qi);
				Assert.AreEqual(10, qi.UserPriority);
			}
			for (var i = 0; i < 100; i++)
			{
				QueueItem qi;
				completed.TryDequeue(out qi);
				Assert.AreEqual(5, qi.UserPriority);
			}
			for (var i = 0; i < 100; i++)
			{
				QueueItem qi;
				completed.TryDequeue(out qi);
				Assert.AreEqual(0, qi.UserPriority);
			}
		}

		[Test]
		public void SomeWorkersTest()
		{
			Action priority10TaskGenerator = () =>
			{
				for (var i = 0; i < 100; i++)
					AddTask(Guid.NewGuid(), up: 10);
			};

			Action priority5TaskGenerator = () =>
			{
				for (var i = 0; i < 100; i++)
					AddTask(Guid.NewGuid(), up: 5);
			};

			Action priority0TaskGenerator = () =>
			{
				for (var i = 0; i < 100; i++)
					AddTask(Guid.NewGuid());
			};

			var tasks = new[] { Task.Run(priority10TaskGenerator), Task.Run(priority5TaskGenerator), Task.Run(priority0TaskGenerator) };
			tasks = tasks.Concat(StartWorker(5, 5)).ToArray();
			Task.WaitAll(tasks);

			Assert.AreEqual(300, completed.Count);
			for (var i = 0; i < 100; i++)
			{
				QueueItem qi;
				completed.TryDequeue(out qi);
				Assert.AreEqual(10, qi.UserPriority);
			}
			for (var i = 0; i < 100; i++)
			{
				QueueItem qi;
				completed.TryDequeue(out qi);
				Assert.AreEqual(5, qi.UserPriority);
			}
			for (var i = 0; i < 100; i++)
			{
				QueueItem qi;
				completed.TryDequeue(out qi);
				Assert.AreEqual(0, qi.UserPriority);
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AGO.WorkQueue
{
	public class InMemoryWorkQueue: AbstractWorkQueue
	{
		private readonly ReaderWriterLockSlim rwlock;
		private readonly List<QueueItem> queue;

		public InMemoryWorkQueue()
		{
			rwlock = new ReaderWriterLockSlim();
			queue = new List<QueueItem>();
		}

		public override void Add(QueueItem item)
		{
			rwlock.EnterWriteLock();
			try
			{
				queue.Add(item);
			}
			finally
			{
				rwlock.ExitWriteLock();
			}
		}

		public override void Remove(Guid taskId)
		{
			rwlock.EnterWriteLock();
			try
			{
				queue.RemoveAll(qi => qi.TaskId == taskId);
			}
			finally
			{
				rwlock.ExitWriteLock();
			}
		}

		public override QueueItem Get(string project)
		{
			rwlock.EnterUpgradeableReadLock();
			try
			{
				//First priority - by user
				var item = queue
					.Where(i => i.Project == project && i.PriorityType != 0)
					.OrderByDescending(i => i.UserPriority)
					.ThenBy(i => i.CreateDate)
					.FirstOrDefault();
				//Second, priority by date in non-user prioritized part of queue
				if (item == null)
				{
					item = queue
						.Where(i => i.Project == project)
						.OrderBy(i => i.CreateDate)
						.FirstOrDefault();
				}
				//Remove item if found - only one worker get this task to execution
				if (item != null)
				{
					rwlock.EnterWriteLock();
					try
					{
						queue.Remove(item);
					}
					finally
					{
						rwlock.ExitWriteLock();
					}
				}
				return item;
			}
			finally
			{
				rwlock.ExitUpgradeableReadLock();
			}
		}

		public override IEnumerable<string> UniqueProjects()
		{
			rwlock.EnterReadLock();
			try
			{
				return queue.Select(i => i.Project).Distinct().ToArray();
			}
			finally
			{
				rwlock.ExitReadLock();
			}
		}

		public override IEnumerable<QueueItem> Dump()
		{
			rwlock.EnterReadLock();
			try
			{
				return queue.ToArray();
			}
			finally
			{
				rwlock.ExitReadLock();
			}
		}

		public override IDictionary<string, IDictionary<string, QueueItem[]>> Snapshot()
		{
			rwlock.EnterReadLock();
			try
			{
				var mapByProject = queue
					.Select(i => i.Project).Distinct()
					.ToDictionary(p => p, p =>
					{
						var withPriority = queue.Where(i => i.Project == p && i.PriorityType != 0)
							.Select(i => i.Copy())
							.OrderByDescending(i => i.UserPriority)
							.ThenBy(i => i.CreateDate);

						var withoutPriority = queue.Where(i => i.Project == p && i.PriorityType == 0)
							.Select(i => i.Copy())
							.OrderBy(i => i.CreateDate);

						return withPriority.Concat(withoutPriority).Select((i, index) =>
						{
							i.OrderInQueue = index + 1;
							return i;
						}).ToList();
					});

				return queue
					.Select(i => i.User).Distinct()
					.ToDictionary<string, string, IDictionary<string, QueueItem[]>>(uid => uid, 
						uid => mapByProject
								.Where(kvp => kvp.Value.Any(i => i.User == uid))
								.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Where(i => i.User == uid).ToArray()));
			}
			finally
			{
				rwlock.ExitReadLock();
			}
		}

		public override void Clear()
		{
			rwlock.EnterWriteLock();
			try
			{
				queue.Clear();
			}
			finally
			{
				rwlock.ExitWriteLock();
			}
		}
	}
}
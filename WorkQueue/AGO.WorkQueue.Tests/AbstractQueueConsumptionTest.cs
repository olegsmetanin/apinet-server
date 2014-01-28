using System;
using System.Linq;
using NUnit.Framework;

namespace AGO.WorkQueue.Tests
{
	[TestFixture]
	public abstract class AbstractQueueConsumptionTest
	{
		protected IWorkQueue Queue;

		protected abstract IWorkQueue CreateQueue();

		[SetUp]
		public virtual void SetUp()
		{
			Queue = CreateQueue();
		}

		[TearDown]
		public virtual void TearDown()
		{
			Queue = null;
		}

		[Test]
		public void QueueReturnItemForRequestedProject()
		{
			const string proj1 = "proj1";
			const string proj2 = "proj2";
			var p1Item = new QueueItem("a", Guid.NewGuid(), proj1, Guid.NewGuid());
			var p2Item = new QueueItem("a", Guid.NewGuid(), proj2, Guid.NewGuid());
			Queue.Add(p2Item);
			Queue.Add(p1Item);

			var item = Queue.Get(proj1);

			Assert.AreEqual(p1Item.Project, item.Project);
			Assert.AreEqual(p1Item.TaskType, item.TaskType);
			Assert.AreEqual(p1Item.TaskId, item.TaskId);
		}

		[Test]
		public void QueueReturnNullForNonExistingProject()
		{
			const string proj1 = "proj1";
			Queue.Add(new QueueItem("a", Guid.NewGuid(), proj1, Guid.NewGuid()));

			var item = Queue.Get("non existing project");

			Assert.IsNull(item);
		}

		[Test]
		public void QueueReturnTaskForUserWithHigherPriority()
		{
			const string proj = "proj1";
			var user1 = Guid.NewGuid();
			var user2 = Guid.NewGuid();
			var i1 = new QueueItem("a", Guid.NewGuid(), proj, user1) { PriorityType = 1, UserPriority = 10 };
			var i2 = new QueueItem("a", Guid.NewGuid(), proj, user2) { PriorityType = 1, UserPriority = 20 };
			Queue.Add(i1);
			Queue.Add(i2);

			var item = Queue.Get(proj);

			Assert.AreEqual(i2.TaskId, item.TaskId);
		}

		[Test]
		public void QueueReturnOldestTaskForUserWhenPrioritiesEquals()
		{
			const string proj = "proj1";
			var uid = Guid.NewGuid();
			var i1 = new QueueItem("a", Guid.NewGuid(), proj, uid) { PriorityType = 1, UserPriority = 10 };
			var i2 = new QueueItem("a", Guid.NewGuid(), proj, uid) { PriorityType = 1, UserPriority = 10 };
			Queue.Add(i1);
			Queue.Add(i2);

			var item = Queue.Get(proj);

			Assert.AreEqual(i1.TaskId, item.TaskId);
		}

		[Test]
		public void QueueReturnOldestTaskWhenPriorityNotUsed()
		{
			const string proj = "proj1";
			var uid = Guid.NewGuid();
			var i1 = new QueueItem("a", Guid.NewGuid(), proj, uid); //PriorityType = 0 by default, so, not used
			var i2 = new QueueItem("a", Guid.NewGuid(), proj, uid);
			Queue.Add(i1);
			Queue.Add(i2);

			var item = Queue.Get(proj);

			Assert.AreEqual(i1.TaskId, item.TaskId);
		}

		[Test]
		public void QueueReturnOldestTaskWhenPriorityNotUsed2()
		{
			const string proj = "proj1";
			var uid = Guid.NewGuid();
			var i1 = new QueueItem("a", Guid.NewGuid(), proj, uid);
			var i2 = new QueueItem("a", Guid.NewGuid(), proj, uid) { UserPriority = 10};
			Queue.Add(i1);
			Queue.Add(i2);

			var item = Queue.Get(proj);

			Assert.AreEqual(i1.TaskId, item.TaskId);
		}

		[Test]
		public void QueueDumpDataAsRawShallowCopyList()
		{
			Queue.Add(new QueueItem("a", Guid.NewGuid(), "p1", Guid.NewGuid()));
			Queue.Add(new QueueItem("a", Guid.NewGuid(), "p2", Guid.NewGuid()));
			Queue.Add(new QueueItem("a", Guid.NewGuid(), "p3", Guid.NewGuid()));

			// ReSharper disable PossibleMultipleEnumeration
			var dump = Queue.Dump(); //special not materialize - test dump is copy, not reference to underlying data

			Assert.AreEqual(3, dump.Count());

			Assert.IsTrue(dump.Any(qi => qi.Project == "p1"));
			Assert.IsTrue(dump.Any(qi => qi.Project == "p2"));
			Assert.IsTrue(dump.Any(qi => qi.Project == "p3"));

			Queue.Clear();

			Assert.AreEqual(3, dump.Count());
			Assert.IsTrue(dump.Any(qi => qi.Project == "p1"));
			Assert.IsTrue(dump.Any(qi => qi.Project == "p2"));
			Assert.IsTrue(dump.Any(qi => qi.Project == "p3"));

			// ReSharper restore PossibleMultipleEnumeration
		}

		[Test]
		public void QueueCalculateOrderForEachUserTaskInProjectWithPriority()
		{
			var u1 = Guid.NewGuid();
			var u2 = Guid.NewGuid();
			const string p1 = "proj1";
			const string p2 = "proj2";
			const string p3 = "proj3";
			var i1 = new QueueItem("a", Guid.NewGuid(), p1, u1){ PriorityType = 1, UserPriority = 20};
			var i2 = new QueueItem("a", Guid.NewGuid(), p1, u1) { PriorityType = 1, UserPriority = 20 };
			var i3 = new QueueItem("a", Guid.NewGuid(), p1, u1);
			var i4 = new QueueItem("a", Guid.NewGuid(), p2, u1) {PriorityType = 1, UserPriority = 10};
			var i5 = new QueueItem("a", Guid.NewGuid(), p2, u1);
			var i6 = new QueueItem("a", Guid.NewGuid(), p1, u2) {PriorityType = 1, UserPriority = 40};
			var i7 = new QueueItem("a", Guid.NewGuid(), p2, u2) {PriorityType = 1, UserPriority = 10};
			var i8 = new QueueItem("a", Guid.NewGuid(), p3, u1);
			var i9 = new QueueItem("a", Guid.NewGuid(), p3, u2);

			Queue.Add(i1);
			Queue.Add(i2);
			Queue.Add(i3);
			Queue.Add(i4);
			Queue.Add(i5);
			Queue.Add(i6);
			Queue.Add(i7);
			Queue.Add(i8);
			Queue.Add(i9);

			var snapshot = Queue.Snapshot();

			/*
			 * u1: priority 20 in proj1
			 * u1: priority 10 in proj2
			 * u2: priority 40 in proj1
			 * u2: priority 10 in proj2
			 * 
			 *			var	PriorityType	Priority	User
			 * proj1	i6		1				40		u2
			 *			i1		1				20		u1
			 *			i2		1				20		u1
			 *			i3		0				--		u1
			 *			
			 * proj2	i4		1				10		u1
			 *			i7		1				10		u2
			 *			i5		0				--		u1
			 *			
			 * proj3	
			 *			i8		0				--		u1
			 *			i9		0				--		u2
			 * 
			 * */
			Assert.IsNotNull(snapshot);
			Assert.AreEqual(2, snapshot.Count);
			//u1 proj1
			Assert.AreEqual(i1.TaskId, snapshot[u1][p1][0].TaskId);
			Assert.AreEqual(i2.TaskId, snapshot[u1][p1][1].TaskId);
			Assert.AreEqual(i3.TaskId, snapshot[u1][p1][2].TaskId);
			Assert.AreEqual(2, snapshot[u1][p1][0].OrderInQueue);
			Assert.AreEqual(3, snapshot[u1][p1][1].OrderInQueue);
			Assert.AreEqual(4, snapshot[u1][p1][2].OrderInQueue);
			//u1 proj2
			Assert.AreEqual(i4.TaskId, snapshot[u1][p2][0].TaskId);
			Assert.AreEqual(i5.TaskId, snapshot[u1][p2][1].TaskId);
			Assert.AreEqual(1, snapshot[u1][p2][0].OrderInQueue);
			Assert.AreEqual(3, snapshot[u1][p2][1].OrderInQueue);
			//u1 proj3
			Assert.AreEqual(i8.TaskId, snapshot[u1][p3][0].TaskId);
			Assert.AreEqual(1, snapshot[u1][p3][0].OrderInQueue);
			//u2 proj1
			Assert.AreEqual(i6.TaskId, snapshot[u2][p1][0].TaskId);
			Assert.AreEqual(1, snapshot[u2][p1][0].OrderInQueue);
			//u2 proj2
			Assert.AreEqual(i7.TaskId, snapshot[u2][p2][0].TaskId);
			Assert.AreEqual(2, snapshot[u2][p2][0].OrderInQueue);
			//u2 proj3
			Assert.AreEqual(i9.TaskId, snapshot[u2][p3][0].TaskId);
			Assert.AreEqual(2, snapshot[u2][p3][0].OrderInQueue);
		}

		[Test]
		public void QueueReturnUniqueProjectCodes()
		{
			const string proj1 = "proj1";
			const string proj2 = "proj2";
			const string proj3 = "proj3";
			Queue.Add(new QueueItem("a", Guid.NewGuid(), proj1, Guid.NewGuid()));
			Queue.Add(new QueueItem("a", Guid.NewGuid(), proj2, Guid.NewGuid()));
			Queue.Add(new QueueItem("a", Guid.NewGuid(), proj1, Guid.NewGuid()));
			Queue.Add(new QueueItem("a", Guid.NewGuid(), proj3, Guid.NewGuid()));
			Queue.Add(new QueueItem("a", Guid.NewGuid(), proj2, Guid.NewGuid()));
			Queue.Add(new QueueItem("a", Guid.NewGuid(), proj1, Guid.NewGuid()));

			var projects = Queue.UniqueProjects().ToArray();

			Assert.AreEqual(3, projects.Length);
			Assert.IsTrue(projects.Contains(proj1));
			Assert.IsTrue(projects.Contains(proj2));
			Assert.IsTrue(projects.Contains(proj3));
		}
	}
}
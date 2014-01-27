using System;
using NUnit.Framework;

namespace AGO.WorkQueue.Tests
{
	[TestFixture]
    public class QueueItemTest
    {
		[Test]
		public void ItemMustContainsRequiredFields()
		{
			var taskId = Guid.NewGuid();
			var userId = Guid.NewGuid();
			var item = new QueueItem("test", taskId, "proj", userId);

			Assert.AreEqual("test", item.TaskType);
			Assert.AreEqual(taskId, item.TaskId);
			Assert.AreEqual("proj", item.Project);
			Assert.AreEqual(userId, item.UserId);
		}

		[Test]
		public void WhenNotSetPropsHasDefaultValue()
		{
			var item = new QueueItem("test", Guid.NewGuid(), "proj", Guid.NewGuid());

			Assert.AreEqual(0, item.PriorityType);
			Assert.AreEqual(0, item.UserPriority);
			Assert.IsTrue(DateTime.UtcNow >= item.CreateDate && DateTime.UtcNow <= item.CreateDate.AddSeconds(1));
		}

		[Test, ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "type", MatchType = MessageMatch.Contains)]
		public void ThrowIfTaskTypeIsNull()
		{
			new QueueItem(null, Guid.Empty, string.Empty, Guid.Empty);
			Assert.Fail("Exception not throwed");
		}

		[Test, ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "type", MatchType = MessageMatch.Contains)]
		public void ThrowIfTaskTypeEmpty()
		{
			new QueueItem(string.Empty, Guid.Empty, string.Empty, Guid.Empty);
			Assert.Fail("Exception not throwed");
		}

		[Test, ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "type", MatchType = MessageMatch.Contains)]
		public void ThrowIfTaskTypeWhitespace()
		{
			new QueueItem("   ", Guid.Empty, string.Empty, Guid.Empty);
			Assert.Fail("Exception not throwed");
		}

		[Test, ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "taskId", MatchType = MessageMatch.Contains)]
		public void ThrowIfTaskIdEmpty()
		{
			new QueueItem("test", default(Guid), string.Empty, Guid.Empty);
			Assert.Fail("Exception not throwed");
		}

		[Test, ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "project", MatchType = MessageMatch.Contains)]
		public void ThrowIfProjectIsNull()
		{
			new QueueItem("test", Guid.NewGuid(), null, Guid.Empty);
			Assert.Fail("Exception not throwed");
		}

		[Test, ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "project", MatchType = MessageMatch.Contains)]
		public void ThrowIfProjectEmpty()
		{
			new QueueItem("test", Guid.NewGuid(), string.Empty, Guid.Empty);
			Assert.Fail("Exception not throwed");
		}

		[Test, ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "project", MatchType = MessageMatch.Contains)]
		public void ThrowIfProjectWhitespace()
		{
			new QueueItem("test", Guid.NewGuid(), "   ", Guid.Empty);
			Assert.Fail("Exception not throwed");
		}

		[Test, ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "userId", MatchType = MessageMatch.Contains)]
		public void ThrowIfUserIdEmpty()
		{
			new QueueItem("test", Guid.NewGuid(), "proj", Guid.Empty);
			Assert.Fail("Exception not throwed");
		}
    }
}

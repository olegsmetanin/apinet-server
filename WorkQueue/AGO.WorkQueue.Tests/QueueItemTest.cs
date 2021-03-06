﻿using System;
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
			const string user = "user1";
			var item = new QueueItem("test", taskId, "proj", user);

			Assert.AreEqual("test", item.TaskType);
			Assert.AreEqual(taskId, item.TaskId);
			Assert.AreEqual("proj", item.Project);
			Assert.AreEqual(user, item.User);
		}

		[Test]
		public void WhenNotSetPropsHasDefaultValue()
		{
			var item = new QueueItem("test", Guid.NewGuid(), "proj", "user");

			Assert.AreEqual(0, item.PriorityType);
			Assert.AreEqual(0, item.UserPriority);
			Assert.IsTrue(DateTime.UtcNow >= item.CreateDate && DateTime.UtcNow <= item.CreateDate.AddSeconds(1));
		}

		[Test, ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "type", MatchType = MessageMatch.Contains)]
		public void ThrowIfTaskTypeIsNull()
		{
			new QueueItem(null, Guid.Empty, string.Empty, string.Empty);
			Assert.Fail("Exception not throwed");
		}

		[Test, ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "type", MatchType = MessageMatch.Contains)]
		public void ThrowIfTaskTypeEmpty()
		{
			new QueueItem(string.Empty, Guid.Empty, string.Empty, string.Empty);
			Assert.Fail("Exception not throwed");
		}

		[Test, ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "type", MatchType = MessageMatch.Contains)]
		public void ThrowIfTaskTypeWhitespace()
		{
			new QueueItem("   ", Guid.Empty, string.Empty, string.Empty);
			Assert.Fail("Exception not throwed");
		}

		[Test, ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "taskId", MatchType = MessageMatch.Contains)]
		public void ThrowIfTaskIdEmpty()
		{
			new QueueItem("test", default(Guid), string.Empty, string.Empty);
			Assert.Fail("Exception not throwed");
		}

		[Test, ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "project", MatchType = MessageMatch.Contains)]
		public void ThrowIfProjectIsNull()
		{
			new QueueItem("test", Guid.NewGuid(), null, string.Empty);
			Assert.Fail("Exception not throwed");
		}

		[Test, ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "project", MatchType = MessageMatch.Contains)]
		public void ThrowIfProjectEmpty()
		{
			new QueueItem("test", Guid.NewGuid(), string.Empty, string.Empty);
			Assert.Fail("Exception not throwed");
		}

		[Test, ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "project", MatchType = MessageMatch.Contains)]
		public void ThrowIfProjectWhitespace()
		{
			new QueueItem("test", Guid.NewGuid(), "   ", string.Empty);
			Assert.Fail("Exception not throwed");
		}

		[Test, ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "user", MatchType = MessageMatch.Contains)]
		public void ThrowIfUserIdEmpty()
		{
			new QueueItem("test", Guid.NewGuid(), "proj", string.Empty);
			Assert.Fail("Exception not throwed");
		}
    }
}

using System;
using System.Linq;
using AGO.Tasks.Model.Task;
using NUnit.Framework;

namespace AGO.Tasks.Test
{
	/// <summary>
	/// Тесты работы со статусами задачи
	/// </summary>
	public class TaskStatusChangeTest: AbstractTest
	{
		[Test]
		public void TaskCreationSetInitialStatusAndAddRecordToHistory()
		{
			var task = M.Task(1);
			_SessionProvider.FlushCurrentSession();

			Assert.AreEqual(TaskStatus.New, task.Status);
			Assert.AreEqual(1, task.StatusHistory.Count);
			var hr = task.StatusHistory.First();
			Assert.AreEqual(TaskStatus.New, hr.Status);
			Assert.AreNotEqual(default(DateTime), hr.Start);
			Assert.IsNull(hr.Finish);
		}

		[Test]
		public void ChangeStatusAddRecordToHistory()
		{
			var task = M.Task(1);
			_SessionProvider.FlushCurrentSession();

			task.ChangeStatus(TaskStatus.Doing, M.MemberFromUser(task.ProjectCode, CurrentUser));

			Assert.AreEqual(TaskStatus.Doing, task.Status);
			Assert.AreEqual(2, task.StatusHistory.Count);
			Assert.IsTrue(task.StatusHistory.Any(h => h.Status == TaskStatus.Doing && h.Finish == null));
		}

		[Test]
		public void ChangeStatusMaintainOnlyOneCurrentRecordInHistory()
		{
			var task = M.Task(1);
			_SessionProvider.FlushCurrentSession();

			task.ChangeStatus(TaskStatus.Doing, M.MemberFromUser(task.ProjectCode, CurrentUser));
			task.ChangeStatus(TaskStatus.Done, M.MemberFromUser(task.ProjectCode, CurrentUser));

			Assert.AreEqual(1, task.StatusHistory.Count(h => h.Finish == null));
		}

		[Test]
		public void ChangeStatusToTheSameDoesNotAddRecordToHistory()
		{
			var task = M.Task(1);
			_SessionProvider.FlushCurrentSession();

			task.ChangeStatus(TaskStatus.New, M.MemberFromUser(task.ProjectCode, CurrentUser));

			Assert.AreEqual(TaskStatus.New, task.Status);
			Assert.AreEqual(1, task.StatusHistory.Count);
			Assert.IsTrue(task.StatusHistory.Any(h => h.Status == TaskStatus.New && h.Finish == null));
		}
	}
}
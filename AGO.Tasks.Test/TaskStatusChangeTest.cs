using System;
using System.Linq;
using AGO.Tasks.Model.Task;
using NUnit.Framework;

namespace AGO.Tasks.Test
{
	/// <summary>
	/// Тесты работы со статусами задачи
	/// </summary>
	[TestFixture]
	public class TaskStatusChangeTest: AbstractTest
	{
		//private TasksController controller; will be need later, when test workflow

		[TestFixtureSetUp]
		public new void Init()
		{
			base.Init();
			//controller = IocContainer.GetInstance<TasksController>();
		}

		[TestFixtureTearDown]
		public new void Cleanup()
		{
			base.Cleanup();
		}

		[TearDown]
		public new void TearDown()
		{
			base.TearDown();
		}


		[Test]
		public void TaskCreationSetInitialStatusAndAddRecordToHistory()
		{
			var task = M.Task(1);
			_SessionProvider.FlushCurrentSession();

			Assert.AreEqual(TaskStatus.NotStarted, task.Status);
			Assert.AreEqual(1, task.StatusHistory.Count);
			var hr = task.StatusHistory.First();
			Assert.AreEqual(TaskStatus.NotStarted, hr.Status);
			Assert.AreNotEqual(default(DateTime), hr.Start);
			Assert.IsNull(hr.Finish);
		}

		[Test]
		public void ChangeStatusAddRecordToHistory()
		{
			var task = M.Task(1);
			_SessionProvider.FlushCurrentSession();

			task.ChangeStatus(TaskStatus.InWork, CurrentUser);

			Assert.AreEqual(TaskStatus.InWork, task.Status);
			Assert.AreEqual(2, task.StatusHistory.Count);
			Assert.IsTrue(task.StatusHistory.Any(h => h.Status == TaskStatus.InWork && h.Finish == null));
		}

		[Test]
		public void ChangeStatusMaintainOnlyOneCurrentRecordInHistory()
		{
			var task = M.Task(1);
			_SessionProvider.FlushCurrentSession();

			task.ChangeStatus(TaskStatus.InWork, CurrentUser);
			task.ChangeStatus(TaskStatus.Completed, CurrentUser);

			Assert.AreEqual(1, task.StatusHistory.Count(h => h.Finish == null));
		}

		[Test]
		public void ChangeStatusToTheSameDoesNotAddRecordToHistory()
		{
			var task = M.Task(1);
			_SessionProvider.FlushCurrentSession();

			task.ChangeStatus(TaskStatus.NotStarted, CurrentUser);

			Assert.AreEqual(TaskStatus.NotStarted, task.Status);
			Assert.AreEqual(1, task.StatusHistory.Count);
			Assert.IsTrue(task.StatusHistory.Any(h => h.Status == TaskStatus.NotStarted && h.Finish == null));
		}
	}
}
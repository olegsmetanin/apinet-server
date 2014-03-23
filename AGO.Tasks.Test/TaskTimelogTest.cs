using System;
using System.Linq;
using AGO.Core.Model.Security;
using AGO.Tasks.Controllers;
using AGO.Tasks.Model.Task;
using NHibernate;
using NUnit.Framework;

namespace AGO.Tasks.Test
{
	/// <summary>
	/// Test for task timelog
	/// </summary>
	public class TaskTimelogTest: AbstractTest
	{
		private TasksController controller;
		private UserModel admin;
		private TaskModel task;

		public override void FixtureSetUp()
		{
			base.FixtureSetUp();

			controller = IocContainer.GetInstance<TasksController>();
			admin = LoginAdmin();
		}

		public override void SetUp()
		{
			base.SetUp();

			task = M.Task(1);
		}

		[Test]
		public void TimelogReturnedInTaskCardDTO()
		{
			var time = M.Time(task, admin, 2, "abc");

			var dto = controller.GetTask(TestProject, task.SeqNumber);

			Assert.That(dto, Is.Not.Null);
			var timelog = dto.Timelog.ToArray();
			Assert.That(timelog, Has.Length.EqualTo(1));
			var entry = timelog[0];
			Assert.That(entry.Id, Is.EqualTo(time.Id));
			Assert.That(entry.Member, Is.EqualTo(time.Member.FullName));
			Assert.That(entry.Time, Is.EqualTo(time.Time));
			Assert.That(entry.Comment, Is.EqualTo(time.Comment));
		}

		[Test]
		public void AddTimeReturnCreatedEntry()
		{
			var dto = controller.TrackTime(TestProject, task.Id, 3m, "qwe");

			Assert.That(dto, Is.Not.Null);
			Assert.That(dto.Id, Is.Not.EqualTo(Guid.Empty));
			Assert.That(dto.Member, Is.EqualTo(admin.FullName));
			Assert.That(dto.Time, Is.EqualTo(3m));
			Assert.That(dto.Comment, Is.EqualTo("qwe"));
		}

		[Test]
		public void DeleteTimeReturnTrue()
		{
			var time = M.Time(task, admin);
			Session.Clear();

// ReSharper disable once AccessToModifiedClosure
			Assert.That(() => controller.DeleteTime(TestProject, time.Id),
				Throws.Nothing);

			time = Session.Get<TaskTimelogEntryModel>(time.Id);
			Assert.That(time, Is.Null);
			task = Session.Get<TaskModel>(task.Id);
			Assert.That(task.Timelog.Count, Is.EqualTo(0));
		}

		[Test]
		public void ChangeTimeReturnChangedEntry()
		{
			var time = M.Time(task, admin, 10m, "abc");

			var dto = controller.UpdateTime(TestProject, time.Id, 20m, "qwe");
			Session.Flush();

			Assert.That(dto.Id, Is.EqualTo(time.Id));
			Assert.That(dto.Time, Is.EqualTo(20m));
			Assert.That(dto.Comment, Is.EqualTo("qwe"));
			time = Session.Get<TaskTimelogEntryModel>(time.Id);
			Assert.That(time.Time, Is.EqualTo(20m));
			Assert.That(time.Comment, Is.EqualTo("qwe"));
		}

		[Test]
		public void TimelogReturnedInTaskCardDTOIfEmpty()
		{
			var dto = controller.GetTask(TestProject, task.SeqNumber);

			Assert.That(dto, Is.Not.Null);
			Assert.That(dto.Timelog, Is.Not.Null);
			var timelog = dto.Timelog.ToArray();
			Assert.That(timelog, Has.Length.EqualTo(0));
		}

		[Test]
		public void ChangeNotExistingTimeThrow()
		{
			Assert.That(() => controller.UpdateTime(TestProject, Guid.NewGuid(), 20m, null),
				Throws.Exception.TypeOf<ObjectNotFoundException>());
		}

		[Test]
		public void DeleteNotExistingTimeThrow()
		{
			Assert.That(() => controller.DeleteTime(TestProject, Guid.NewGuid()),
				Throws.Exception.TypeOf<ObjectNotFoundException>());
		}
	}
}
using System;
using System.Globalization;
using System.Linq;
using AGO.Core.Filters;
using AGO.Core.Model.Security;
using AGO.Home;
using AGO.Home.Model.Projects;
using AGO.Tasks.Controllers;
using AGO.Tasks.Controllers.DTO;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using NUnit.Framework;

namespace AGO.Tasks.Test
{
	/// <summary>
	/// Тесты CRUD реестра задач
	/// </summary>
	[TestFixture]
	public class TaskCRUDTest: AbstractTest
	{
		private TasksController controller;

		[SetUp]
		public new void Init()
		{
			base.Init();
			controller = _Container.GetInstance<TasksController>();
		}

		[TearDown]
		public new void Cleanup()
		{
			base.Cleanup();
		}

		private UserModel Admin
		{
			get { return Session.QueryOver<UserModel>().Where(m => m.LastName == "admin@agosystems.com").SingleOrDefault(); }
		}

		private TaskTypeModel MakeType(string name)
		{
			var type = new TaskTypeModel
			       	{
			       		Creator = Admin,
			       		ProjectCode = TestProject,
			       		Name = name
			       	};
			Session.Save(type);
			return type;
		}

		private TaskModel Make(int num, TaskTypeModel type, string content = null, TaskStatus status = TaskStatus.NotStarted)
		{
			var task = new TaskModel
			           	{
			           		Creator = Admin,
			           		ProjectCode = TestProject,
			           		InternalSeqNumber = num,
			           		SeqNumber = "t0-" + num,
			           		TaskType = type,
			           		Content = content,
			           		Status = status
			           	};
			Session.Save(task);
			return task;
		}

		[Test]
		public void GetTasksReturnAllRecords()
		{
			var tt = MakeType("tt");
			var t1 = Make(1, tt);
			var t2 = Make(2, tt);
			_SessionProvider.CloseCurrentSession();

			var result = controller.GetTasks(
				TestProject,
				Enumerable.Empty<IModelFilterNode>().ToArray(),
				new[] {new SortInfo {Property = "InternalSeqNumber"}},
				0, 10).ToArray();

			Assert.AreEqual(2, result.Length);
			Assert.AreEqual(t1.Id, result[0].Id);
			Assert.AreEqual(t2.Id, result[1].Id);
		}

		[Test]
		public void LookupTasksWithoutTermReturnAllRecords()
		{
			var tt = MakeType("tt");
			var t1 = Make(1, tt);
			var t2 = Make(2, tt);
			_SessionProvider.CloseCurrentSession();

			var result = controller.LookupTasks(
				TestProject,
				null,
				0, 10).ToArray();

			//assume order by InternalSeqNumber as default sort
			Assert.AreEqual(2, result.Length);
			StringAssert.AreEqualIgnoringCase(t1.Id.ToString(), result[0].Id);
			StringAssert.AreEqualIgnoringCase(t1.SeqNumber, result[0].Text);
			StringAssert.AreEqualIgnoringCase(t2.Id.ToString(), result[1].Id);
			StringAssert.AreEqualIgnoringCase(t2.SeqNumber, result[1].Text);
		}

		[Test, ExpectedException(typeof(NoSuchProjectException))]
		public void CreateTaskWithoutProjectThrow()
		{
			var model = new CreateTaskDTO();

			controller.CreateTask("not existed project", model);
		}

		[Test]
		public void CreateTaskWithoutTypeReturnError()
		{
			var model = new CreateTaskDTO();

			var vr = controller.CreateTask(TestProject, model);

			Assert.IsFalse(vr.Success);
			Assert.IsTrue(vr.FieldErrors.First(e => e.Key == "TaskType").Value.Any());
		}

		[Test]
		public void CreateTaskWithWrongTypeReturnError()
		{
			var model = new CreateTaskDTO { TaskType = Guid.NewGuid(), Executors = new [] { Guid.NewGuid()}};

			var vr = controller.CreateTask(TestProject, model);

			Assert.IsFalse(vr.Success);
			Assert.IsTrue(vr.FieldErrors.First(e => e.Key == "TaskType").Value.Any());
		}

		[Test]
		public void CreateTaskWithoutExecutorsReturnError()
		{
			var tt = MakeType("tt");
			_SessionProvider.CloseCurrentSession();
			var model = new CreateTaskDTO {TaskType = tt.Id};

			var vr = controller.CreateTask(TestProject, model);

			Assert.IsFalse(vr.Success);
			Assert.IsTrue(vr.FieldErrors.First(e => e.Key == "Executors").Value.Any());

			model.Executors = new Guid[0];

			vr = controller.CreateTask(TestProject, model);

			Assert.IsFalse(vr.Success);
			Assert.IsTrue(vr.FieldErrors.First(e => e.Key == "Executors").Value.Any());
		}

		[Test]
		public void CreateTaskWithWrongExecutorsReturnError()
		{
			var tt = MakeType("tt");
			_SessionProvider.CloseCurrentSession();
			var model = new CreateTaskDTO { TaskType = tt.Id, Executors = new [] { Guid.NewGuid() } };

			var vr = controller.CreateTask(TestProject, model);

			Assert.IsFalse(vr.Success);
			Assert.IsTrue(vr.FieldErrors.First(e => e.Key == "Executors").Value.Any());
		}

		[Test]
		public void CreateTaskWithValidParamsReturnSuccess()
		{
			var tt = MakeType("tt");
			var status = new CustomTaskStatusModel
			             	{
			             		ProjectCode = TestProject,
			             		Creator = Admin,
			             		Name = "s"
			             	};
			Session.Save(status);
			var project = Session.QueryOver<ProjectModel>().Where(m => m.ProjectCode == TestProject).SingleOrDefault();
			var participant = project.Participants.First();
			_SessionProvider.CloseCurrentSession();

			var model = new CreateTaskDTO
			            	{
			            		TaskType = tt.Id,
			            		Executors = new[] {participant.Id},
			            		DueDate = new DateTime(2013, 01, 01),
			            		Content = "test task",
			            		CustomStatus = status.Id,
			            		Priority = TaskPriority.Low
			            	};
			var vr = controller.CreateTask(TestProject, model);
			_SessionProvider.CloseCurrentSession();

			Assert.IsTrue(vr.Success);
			var task = Session.QueryOver<TaskModel>().Where(m => m.ProjectCode == TestProject).Take(1).SingleOrDefault();
			Assert.IsNotNull(task);
			Assert.AreEqual(TestProject, task.ProjectCode);
			Assert.AreEqual("t0-1", task.SeqNumber);
			Assert.AreEqual(1, task.InternalSeqNumber);
			Assert.AreEqual(tt.Id, task.TaskType.Id);
			Assert.IsTrue(task.Executors.Any(e => e.Executor.Id == participant.Id));
			Assert.AreEqual(new DateTime(2013, 01, 01).ToUniversalTime(), task.DueDate);
			Assert.AreEqual("test task", task.Content);
			Assert.AreEqual(status.Id, task.CustomStatus.Id);
			Assert.IsTrue(task.CustomStatusHistory.Any(h => h.Task.Id == task.Id && h.Status.Id == status.Id));
			Assert.AreEqual(TaskPriority.Low, task.Priority);
		}
	}
}
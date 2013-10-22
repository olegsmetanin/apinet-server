using System;
using System.Linq;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Tasks.Controllers.DTO;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using NUnit.Framework;

namespace AGO.Tasks.Test
{
	/// <summary>
	/// Тесты CRUD справочника типов задач
	/// </summary>
	[TestFixture]
	public class TaskTypeCRUDTest: AbstractDictionaryTest
	{
		[TestFixtureSetUp]
		public new void Init()
		{
			base.Init();
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
		public void LookupTaskTypesWithoutTermReturnAll()
		{
			M.TaskType("tt1");
			M.TaskType("tt2");
			_SessionProvider.FlushCurrentSession();

			var result = Controller.LookupTaskTypes(TestProject, null, 0).ToArray();

			Assert.AreEqual(2, result.Length);
		}

		[Test]
		public void ReadTaskTypesFromEmptyReturnEmptyData()
		{
			var result = Controller.GetTaskTypes(TestProject, 
				Enumerable.Empty<IModelFilterNode>().ToArray(), 
				Enumerable.Empty<SortInfo>().ToArray(), 
				0);

			Assert.IsNotNull(result);
			Assert.IsFalse(result.Any());
		}
		
		[Test]
		public void ReadTaskTypes()
		{
			M.TaskType("tt1");
			M.TaskType("tt2");
			_SessionProvider.FlushCurrentSession();

			var result = Controller.GetTaskTypes(
				TestProject,
				Enumerable.Empty<IModelFilterNode>().ToArray(),
				new [] { new SortInfo {Property = "Name"} }, //need ordered result for assertion
				0).ToArray(); 

			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.Length);
			Assert.AreEqual("tt1", result[0].Name);
			Assert.AreEqual("tt2", result[1].Name);
		}
		
		[Test]
		public void CreateTaskType()
		{
			var model = new TaskTypeDTO {Name = "TestTaskType"};

			var vr = Controller.EditTaskType(TestProject, model).Validation;
			_SessionProvider.FlushCurrentSession(!vr.Success);

			var tt = _SessionProvider.CurrentSession.QueryOver<TaskTypeModel>()
				.Where(m => m.ProjectCode == TestProject && m.Name == "TestTaskType")
				.SingleOrDefault();
			Assert.AreNotEqual(default(Guid), tt.Id);
			Assert.AreEqual("TestTaskType", tt.Name);
			Assert.IsTrue(vr.Success);
		}

		[Test]
		public void CreateTaskTypeWithEmptyNameReturnError()
		{
			var model = new TaskTypeDTO { Name = string.Empty };

			var vr = Controller.EditTaskType(TestProject, model).Validation;
			_SessionProvider.FlushCurrentSession(!vr.Success);

			var tt = _SessionProvider.CurrentSession.QueryOver<TaskTypeModel>()
				.Where(m => m.ProjectCode == TestProject && m.Name == string.Empty)
				.SingleOrDefault();
			Assert.IsNull(tt);
			Assert.IsFalse(vr.Success);
			Assert.IsTrue(vr.FieldErrors.First(e => e.Key == "Name").Value.Any());
		}

		
		[Test]
		public void UpdateTaskType()
		{
			var testTaskType = M.TaskType();
			_SessionProvider.FlushCurrentSession();

			var model = new TaskTypeDTO {Id = testTaskType.Id, Name = "NewName"};
			var vr = Controller.EditTaskType(TestProject, model).Validation;
			_SessionProvider.FlushCurrentSession(!vr.Success);

			testTaskType = _SessionProvider.CurrentSession.Get<TaskTypeModel>(testTaskType.Id);
			Assert.AreEqual("NewName", testTaskType.Name);
			Assert.IsTrue(vr.Success);
		}

		[Test]
		public void UpdateTaskTypeWithEmptyNameReturnError()
		{
			var tt = M.TaskType("aaa");
			_SessionProvider.FlushCurrentSession();

			var model = new TaskTypeDTO { Id = tt.Id, Name = string.Empty };
			var vr = Controller.EditTaskType(TestProject, model).Validation;
			_SessionProvider.FlushCurrentSession(!vr.Success);

			Assert.IsFalse(vr.Success);
			tt = Session.Get<TaskTypeModel>(tt.Id);
			Assert.AreEqual("aaa", tt.Name);
			Assert.IsTrue(vr.FieldErrors.First(e => e.Key == "Name").Value.Any());
		}

		[Test]
		public void DeleteTaskType()
		{
			var testTaskType = M.TaskType();
			_SessionProvider.FlushCurrentSession();

			Controller.DeleteTaskType(testTaskType.Id);
			_SessionProvider.FlushCurrentSession();

			var notExisted = _SessionProvider.CurrentSession.Get<TaskTypeModel>(testTaskType.Id);
			Assert.IsNull(notExisted);
		}

		[Test, ExpectedException(typeof(CannotDeleteReferencedItemException))]
		public void CantDeleteReferencedTaskType()
		{
			var testTaskType = M.TaskType();
			var testTask = new TaskModel
			               	{
			               		ProjectCode = TestProject, 
								SeqNumber = "t0-1",
								InternalSeqNumber = 1,
								TaskType = testTaskType
			               	};
			_CrudDao.Store(testTask);
			_SessionProvider.FlushCurrentSession();
			
			Controller.DeleteTaskType(testTaskType.Id);
		}
	}
}
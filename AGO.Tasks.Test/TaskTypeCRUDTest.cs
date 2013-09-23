using System;
using System.Linq;
using AGO.Core;
using AGO.Core.Application;
using AGO.Core.Filters;
using AGO.Tasks.Controllers.DTO;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using NUnit.Framework;
using DictionaryController = AGO.Tasks.Controllers.DictionaryController;

namespace AGO.Tasks.Test
{
	/// <summary>
	/// Тесты CRUD справочника типов задач
	/// </summary>
	[TestFixture]
	public class TaskTypeCRUDTest: AbstractTestFixture
	{
		private string testProject;
		private DictionaryController taskTypeController;

		[SetUp]
		public void Init()
		{
			InitContainer();
			testProject = Guid.NewGuid().ToString().Replace("-", string.Empty);
			taskTypeController = _Container.GetInstance<DictionaryController>();
		}

		[TearDown]
		public void Cleanup()
		{
			var conn = _SessionProvider.CurrentSession.Connection; 
			ExecuteNonQuery(string.Format(@"
					delete from Tasks.TaskModel where ProjectCode = '{0}'
					go
					delete from Tasks.TaskTypeModel where ProjectCode = '{0}'
					go", testProject), conn);
			_SessionProvider.CloseCurrentSession();
		}

		[Test]
		public void ReadTaskTypesFromEmptyReturnEmptyData()
		{
			var result = taskTypeController.GetTaskTypes(testProject, 
				Enumerable.Empty<IModelFilterNode>().ToArray(), 
				Enumerable.Empty<SortInfo>().ToArray(), 
				0, 10);

			Assert.IsNotNull(result);
			Assert.IsFalse(result.Any());
		}
		
		[Test]
		public void ReadTaskTypes()
		{
			var tt1 = new TaskTypeModel { ProjectCode = testProject, Name = "tt1" };
			var tt2 = new TaskTypeModel { ProjectCode = testProject, Name = "tt2" };
			_SessionProvider.CurrentSession.Save(tt1);
			_SessionProvider.CurrentSession.Save(tt2);
			_SessionProvider.CloseCurrentSession();

			var result = taskTypeController.GetTaskTypes(
				testProject,
				Enumerable.Empty<IModelFilterNode>().ToArray(),
				new [] { new SortInfo {Property = "Name"} }, //need ordered result for assertion
				0, 10).ToArray(); 

			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.Length);
			Assert.AreEqual("tt1", result[0].Name);
			Assert.AreEqual("tt2", result[1].Name);
		}
		
		[Test]
		public void CreateTaskType()
		{
			var model = new TaskTypeDTO { Name = "TestTaskType" };

			var vr = taskTypeController.EditTaskType(testProject, model);
			_SessionProvider.CloseCurrentSession();

			var tt = _SessionProvider.CurrentSession.QueryOver<TaskTypeModel>()
				.Where(m => m.ProjectCode == testProject && m.Name == "TestTaskType")
				.SingleOrDefault();
			Assert.AreNotEqual(default(Guid), tt.Id);
			Assert.AreEqual("TestTaskType", tt.Name);
			Assert.IsTrue(vr.Success);
		}

		
		[Test]
		public void UpdateTaskType()
		{
			var testTaskType = new TaskTypeModel { ProjectCode = testProject, Name = "TestTaskType" };
			_SessionProvider.CurrentSession.Save(testTaskType);
			_SessionProvider.CloseCurrentSession();

			var model = new TaskTypeDTO {Id = testTaskType.Id, Name = "NewName"};
			var vr = taskTypeController.EditTaskType(testProject, model);
			_SessionProvider.CloseCurrentSession();

			testTaskType = _SessionProvider.CurrentSession.Get<TaskTypeModel>(testTaskType.Id);
			Assert.AreEqual("NewName", testTaskType.Name);
			Assert.IsTrue(vr.Success);
		}

		[Test]
		public void DeleteTaskType()
		{
			var testTaskType = new TaskTypeModel {ProjectCode = testProject, Name = "TestTaskType"};
			_SessionProvider.CurrentSession.Save(testTaskType);
			_SessionProvider.CloseCurrentSession();

			taskTypeController.DeleteTaskType(testTaskType.Id);
			_SessionProvider.CloseCurrentSession();

			var notExisted = _SessionProvider.CurrentSession.Get<TaskTypeModel>(testTaskType.Id);
			Assert.IsNull(notExisted);
		}

		[Test, ExpectedException(typeof(CannotDeleteReferencedItemException))]
		public void CantDeleteReferencedTaskType()
		{
			var testTaskType = new TaskTypeModel { ProjectCode = testProject, Name = "TestTaskType" };
			var testTask = new TaskModel
			               	{
			               		ProjectCode = testProject, 
								SeqNumber = "t0-1",
								InternalSeqNumber = 1,
								TaskType = testTaskType
			               	};
			_SessionProvider.CurrentSession.Save(testTaskType);
			_SessionProvider.CurrentSession.Save(testTask);
			_SessionProvider.CloseCurrentSession();
			
			taskTypeController.DeleteTaskType(testTaskType.Id);
		}
	}
}

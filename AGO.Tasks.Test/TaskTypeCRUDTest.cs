using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core;
using AGO.Core.Application;
using AGO.Core.Filters;
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
	public class TaskTypeCRUDTest: AbstractApplication
	{
		protected override void Register()
		{
			RegisterEnvironment();
			RegisterPersistence();
			RegisterControllers();
		}

		protected override void AfterSingletonsInitialized(IList<IInitializable> initializedServices)
		{
			InitializeEnvironment(initializedServices);
			InitializePersistence(initializedServices);
		}
		
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
			var result = taskTypeController.GetTaskTypes(0, 10, 
				Enumerable.Empty<IModelFilterNode>().ToArray(), 
				Enumerable.Empty<SortInfo>().ToArray());

			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.totalRowsCount);
			Assert.IsFalse(result.rows.Any());
		}
		
		[Test]
		public void ReadTaskTypes()
		{
			var tt1 = new TaskTypeModel { ProjectCode = testProject, Name = "tt1" };
			var tt2 = new TaskTypeModel { ProjectCode = testProject, Name = "tt2" };
			_SessionProvider.CurrentSession.Save(tt1);
			_SessionProvider.CurrentSession.Save(tt2);
			_SessionProvider.CloseCurrentSession();
			
			var result = taskTypeController.GetTaskTypes(0, 10, 
				Enumerable.Empty<IModelFilterNode>().ToArray(),
				new [] { new SortInfo {Property = "Name"} }); //need ordered result for assertion

			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.totalRowsCount);
			Assert.AreEqual(2, result.rows.Count());
			Assert.AreEqual("tt1", result.rows.ElementAt(0).Name);
			Assert.AreEqual("tt2", result.rows.ElementAt(1).Name);
		}
		
		[Test]
		public void CreateTaskType()
		{
			var testTaskType = new TaskTypeModel { Name = "TestTaskType" };

			var vr = taskTypeController.EditTaskType(testTaskType, testProject);
			_SessionProvider.CloseCurrentSession();

			testTaskType = _SessionProvider.CurrentSession.QueryOver<TaskTypeModel>()
				.Where(m => m.ProjectCode == testProject && m.Name == "TestTaskType")
				.SingleOrDefault();
			Assert.AreNotEqual(default(Guid), testTaskType.Id);
			Assert.AreEqual("TestTaskType", testTaskType.Name);
			Assert.IsTrue(vr.Success);
		}

		
		[Test]
		public void UpdateTaskType()
		{
			var testTaskType = new TaskTypeModel { ProjectCode = testProject, Name = "TestTaskType" };
			_SessionProvider.CurrentSession.Save(testTaskType);
			_SessionProvider.CloseCurrentSession();

			testTaskType = new TaskTypeModel {Id = testTaskType.Id, Name = "NewName"};
			var vr = taskTypeController.EditTaskType(testTaskType, testProject);
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

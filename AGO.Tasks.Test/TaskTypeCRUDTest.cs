using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AGO.Core;
using AGO.Core.Application;
using AGO.Core.Controllers;
using AGO.Core.Json;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
		public void TaskTypeControllerRegistered()
		{
			Assert.IsNotNull(taskTypeController);
		}

		private string ToJson(object item)
		{
			var serializer = new JsonSerializer();
			var sw = new StringWriter();
			serializer.Serialize(sw, item);
			return sw.ToString();
		}

		private JsonReader Reader(string json)
		{
			return new JsonTextReader(new StringReader(json));
		}

		private JsonReader ModelRequest(string project, string method, object model)
		{
			var serializer = new JsonSerializer();
			var sw = new StringWriter();
			serializer.Serialize(sw, new { project, method, model });
			return Reader(sw.ToString());
		}

		private JsonReader ModelRequest(string project, string method, Guid id)
		{
			var serializer = new JsonSerializer();
			var sw = new StringWriter();
			serializer.Serialize(sw, new { project, method, id });
			return Reader(sw.ToString());
		}

		private JsonReader ModelRequest(string project, string method,
			object simpleFilter = null, object complexFilter = null, object userFilter = null,
			IEnumerable sorters = null, int page = 0, int pageSize = 10)
		{
			var s = new JsonSerializerSettings() {NullValueHandling = NullValueHandling.Ignore};
			var serializer = JsonSerializer.Create(s);
			var sw = new StringWriter();
			serializer.Serialize(sw, new { project, method, 
				filter = new {simple = simpleFilter, complex = complexFilter, user = userFilter},
				sorters, page, pageSize});
			return Reader(sw.ToString());
		}

		private JObject ParseOutput(string output)
		{
			return JObject.Load(Reader(output));
		}

		[Test]
		public void ReadTaskTypesFromEmptyReturnEmptyData()
		{
			//arrange
			var input = ModelRequest(testProject, "tasks/dictionary/getTaskTypes");

			//act
			var result = taskTypeController.GetTaskTypes(input);

			//assert
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
			//need ordered result for assertion
			var input = ModelRequest(testProject, "tasks/dictionary/getTaskTypes", sorters: new [] { new {property = "Name"} });

			var result = taskTypeController.GetTaskTypes(input);

			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.totalRowsCount);
			Assert.AreEqual(2, result.rows.Count());
			Assert.AreEqual("tt1", result.rows.ElementAt(0).Name);
			Assert.AreEqual("tt2", result.rows.ElementAt(1).Name);
		}
		
		[Test]
		public void CreateTaskType()
		{
			var testTaskType = new TaskTypeModel { ProjectCode = testProject, Name = "TestTaskType" };
			var input = ModelRequest(testProject, "tasks/dictionary/editTaskType", testTaskType);

			var vr = taskTypeController.EditTaskType(input);
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

			testTaskType.Name = "NewName";
			var input = ModelRequest(testProject, "tasks/dictionary/editTaskType", testTaskType);

			var vr = taskTypeController.EditTaskType(input);
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
			var input = ModelRequest(testProject, "tasks/dictionary/deleteTaskTypes", testTaskType.Id);

			taskTypeController.DeleteTaskType(input);
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
			var input = ModelRequest(testProject, "tasks/dictionary/deleteTaskTypes", testTaskType.Id);
			
			taskTypeController.DeleteTaskType(input);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AGO.Core;
using AGO.Core.Application;
using AGO.Core.Json;
using AGO.Tasks.Controllers;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

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

			_Container.RegisterSingle<IJsonRequestService, JsonRequestService>();
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
			var cmd = _SessionProvider.CurrentSession.Connection.CreateCommand();

			cmd.CommandText = string.Format("delete from Tasks.TaskModel where ProjectCode = '{0}'", testProject);
			cmd.ExecuteNonQuery();

			cmd.CommandText = string.Format("delete from Tasks.TaskTypeModel where ProjectCode = '{0}'", testProject);
			cmd.ExecuteNonQuery();

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
			var sw = new StringWriter();
			var output = new JsonTextWriter(sw);

			//act
			taskTypeController.GetTaskTypes(input, output);

			//assert
			var result = sw.ToString();
			Assert.IsNotNullOrEmpty(result);
			var jres = ParseOutput(result);
			Assert.AreEqual(0, jres.Property("totalRowsCount").Value.Value<int>());
			Assert.AreEqual(0, (jres.Property("rows").Value as JArray).Count());
		}

		[Test]
		public void ReadTaskTypes()
		{
			var tt1 = new TaskTypeModel { ProjectCode = testProject, Name = "tt1" };
			var tt2 = new TaskTypeModel { ProjectCode = testProject, Name = "tt2" };
			_SessionProvider.CurrentSession.Save(tt1);
			_SessionProvider.CurrentSession.Save(tt2);
			_SessionProvider.CloseCurrentSession();
			//need ordered result for assertion in json format
			var input = ModelRequest(testProject, "tasks/dictionary/getTaskTypes", sorters: new [] { new {property = "Name"} });
			var sw = new StringWriter();
			var output = new JsonTextWriter(sw);

			taskTypeController.GetTaskTypes(input, output);

			//need to reload models, because when saving to underlying database datetime fields 
			//lose their accuracy and, so, json does not match with controller result
			tt1 = _SessionProvider.CurrentSession.Get<TaskTypeModel>(tt1.Id);
			tt2 = _SessionProvider.CurrentSession.Get<TaskTypeModel>(tt2.Id);
			var tt1Json = ToJson(tt1);
			var tt2Json = ToJson(tt2);

			var result = sw.ToString();
			Assert.IsNotNullOrEmpty(result);
			Assert.AreEqual(string.Format("{{\"totalRowsCount\":2,\"rows\":[{0},{1}]}}", tt1Json, tt2Json), result);
		}

		[Test]
		public void CreateTaskType()
		{
			var testTaskType = new TaskTypeModel { ProjectCode = testProject, Name = "TestTaskType" };
			var input = ModelRequest(testProject, "tasks/dictionary/editTaskType", testTaskType);
			var sw = new StringWriter();
			var output = new JsonTextWriter(sw);

			taskTypeController.EditTaskType(input, output);
			_SessionProvider.CloseCurrentSession();

			testTaskType = _SessionProvider.CurrentSession.QueryOver<TaskTypeModel>()
				.Where(m => m.ProjectCode == testProject && m.Name == "TestTaskType")
				.SingleOrDefault();
			Assert.AreNotEqual(default(Guid), testTaskType.Id);
			Assert.AreEqual("TestTaskType", testTaskType.Name);
		}

		[Test]
		public void UpdateTaskType()
		{
			var testTaskType = new TaskTypeModel { ProjectCode = testProject, Name = "TestTaskType" };
			_SessionProvider.CurrentSession.Save(testTaskType);
			_SessionProvider.CloseCurrentSession();

			testTaskType.Name = "NewName";
			var input = ModelRequest(testProject, "tasks/dictionary/editTaskType", testTaskType);
			var sw = new StringWriter();
			var output = new JsonTextWriter(sw);

			taskTypeController.EditTaskType(input, output);
			_SessionProvider.CloseCurrentSession();

			testTaskType = _SessionProvider.CurrentSession.Get<TaskTypeModel>(testTaskType.Id);
			Assert.AreEqual("NewName", testTaskType.Name);
		}

		[Test]
		public void DeleteTaskType()
		{
			var testTaskType = new TaskTypeModel {ProjectCode = testProject, Name = "TestTaskType"};
			_SessionProvider.CurrentSession.Save(testTaskType);
			_SessionProvider.CloseCurrentSession();
			var input = ModelRequest(testProject, "tasks/dictionary/deleteTaskTypes", testTaskType.Id);
			var sw = new StringWriter();
			var output = new JsonTextWriter(sw);

			taskTypeController.DeleteTaskType(input, output);
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
			var sw = new StringWriter();
			var output = new JsonTextWriter(sw);

			taskTypeController.DeleteTaskType(input, output);
			_SessionProvider.CloseCurrentSession();
		}
	}
}

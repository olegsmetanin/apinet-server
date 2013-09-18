using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AGO.Core;
using AGO.Core.Application;
using AGO.Core.Json;
using AGO.Tasks.Controllers;
using NUnit.Framework;
using Newtonsoft.Json;

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

		protected override void DoMigrateUp()
		{
			//skip this step
		}

//		protected override void AfterContainerInitialized(IList<IInitializable> initializedServices)
//		{
//			if (_MigrationService == null) return;
//
//			var now = DateTime.Now;
//			var mostRecentVersion = new Version(now.Year, now.Month, now.Day, 99);
//			_MigrationService.MigrateUp(mostRecentVersion);
//		}

		private DictionaryController taskTypeController;

		[SetUp]
		public void Init()
		{
			InitContainer();

			taskTypeController = _Container.GetInstance<DictionaryController>();
		}

		[Test]
		public void TaskTypeControllerRegistered()
		{
			Assert.IsNotNull(taskTypeController);
		}

		private JsonReader Input(string json)
		{
			return new JsonTextReader(new StringReader(json));
		}

		private JsonReader ModelRequest(string project, string method,
			string simpleFilter = null, string complexFilter = null, string userFilter = null,
			string sorters = null, int page = 1, int pageSize = 10)
		{
			var sb = new StringBuilder();
			sb.Append("{ \"project\": \"").Append(project).Append("\", \"method\": \"").Append(method).Append("\"");
			if (simpleFilter != null || complexFilter != null || userFilter != null)
			{
				sb.Append(", \"filter\": {");
				var firstFilter = true;
				if (simpleFilter != null)
				{
					sb.Append("\"simple\": {").Append(simpleFilter).Append("}");
					firstFilter = false;
				}
				if (complexFilter != null)
				{
					if (!firstFilter)
						sb.Append(", ");
					else
						firstFilter = false;
					sb.Append("\"complex\": {").Append(complexFilter).Append("}");
				}
				if (userFilter != null)
				{
					if (!firstFilter)
						sb.Append(", ");
					sb.Append("\"user\": {").Append(userFilter).Append("}");
				}
				sb.Append("}");
			}

			if (sorters != null)
				sb.Append(", \"sorters\": [").Append(sorters).Append("]");

			sb.Append(", \"page\": ").Append(page).Append(", \"pageSize\": ").Append(pageSize);
			sb.Append("}");

			return Input(sb.ToString());
		}

		[Test]
		public void ReadRecordsFromEmptyTest()
		{
			var input = ModelRequest("t", "tasks/dictionary/getTaskTypes");
			var sw = new StringWriter();
			var output = new JsonTextWriter(sw);

			taskTypeController.GetTaskTypes(input, output);

			var result = sw.ToString();
			Assert.IsNotNullOrEmpty(result);
		}
	}
}

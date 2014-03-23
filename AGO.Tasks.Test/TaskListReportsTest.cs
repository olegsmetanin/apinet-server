using System.Linq;
using System.Xml;
using AGO.Core.Filters;
using AGO.Tasks.Controllers;
using AGO.Tasks.Model.Task;
using AGO.Tasks.Reports;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace AGO.Tasks.Test
{
	/// <summary>
	/// Тесты генераторов данных для отчетов-списков задач
	/// </summary>
	public class TaskListReportsTest: AbstractTest
	{
		private System.Threading.CancellationToken token;
		public override void FixtureSetUp()
		{
			base.FixtureSetUp();
	
			var cts = new System.Threading.CancellationTokenSource();
			token = cts.Token;
		}

		private TaskListReportParameters MakeParameters(
			dynamic filter = null,
			TaskPredefinedFilter predef = TaskPredefinedFilter.All)
		{
			filter = filter ?? new
			{
				simple = new
				{
				    Combined = "All",
					Status = new { path = "Status", op = "=", value = new string[0]},
					SeqNumber = new {path = "Id", op = "=", value = new string[0]},
					TaskType = new {path = "TaskType", op = "=", value = new string[0]},
					Executors = new {path = "Executors.Id", op = "=", value = new string[0]},
				},

				complex = new { op = "&&", items = new string[0]},

				user = new { op = "&&", items = new string[0] }
			};
			return new TaskListReportParameters
			{
			    Project = TestProject,
			    Predefined = predef,
			    Filter = JObject.Parse(JsonConvert.SerializeObject(filter)),
			    Sorters = new[] {new SortInfo {Property = "InternalSeqNumber", Descending = true}}
			};
		}

		private static string Value(XmlNode elm, string marker)
		{
			var valueNode = elm.SelectSingleNode("value[@name=\"" + marker + "\"]");
			if (valueNode == null)
				Assert.Fail("No value node for marker");
			if (valueNode.ChildNodes.Count <= 0)
				Assert.Fail("No child nodes in value node");

			return valueNode.FirstChild.Value;
		}

		private SimpleTaskListDataGenerator MakeSimpleGenerator()
		{
			return new SimpleTaskListDataGenerator(_FilteringService, DaoFactory);
		}

		[Test]
		public void ReportWithDefaultParamsReturnAllRecords()
		{
			var tt1 = M.TaskType("aaa");
			var tt2 = M.TaskType("bbb");
			var t1 = M.Task(1, tt1);
			var t2 = M.Task(2, tt2);

			var gen = MakeSimpleGenerator();

			var doc = gen.GetReportData(MakeParameters(), token);

			Assert.IsNotNull(doc);
			var range = doc.SelectSingleNode("/reportData/range[@name=\"data\"]");
			Assert.IsNotNull(range);
			var items = range.SelectNodes("item");
			Assert.IsNotNull(items);
			Assert.AreEqual(2, items.Count);

			t1 = ProjectSession(TestProject).Load<TaskModel>(t1.Id);
			t2 = ProjectSession(TestProject).Load<TaskModel>(t2.Id);

// ReSharper disable PossibleNullReferenceException
			Assert.AreEqual(t2.SeqNumber, items[0].SelectSingleNode("value[@name=\"num\"]").FirstChild.Value);

			Assert.AreEqual(t2.TaskType.Name, items[0].SelectSingleNode("value[@name=\"type\"]").FirstChild.Value);
			Assert.AreEqual(t2.Executors.First().Executor.FullName, 
				items[0].SelectSingleNode("value[@name=\"executors\"]").FirstChild.Value);

			Assert.AreEqual(t1.SeqNumber, items[1].SelectSingleNode("value[@name=\"num\"]").FirstChild.Value);
			Assert.AreEqual(t1.TaskType.Name, items[1].SelectSingleNode("value[@name=\"type\"]").FirstChild.Value);
			Assert.AreEqual(t1.Executors.First().Executor.FullName, 
				items[1].SelectSingleNode("value[@name=\"executors\"]").FirstChild.Value);
// ReSharper restore PossibleNullReferenceException
		}

		private DetailedTaskListWithCustomPropsDataGenerator MakePropsDataGenerator()
		{
			return new DetailedTaskListWithCustomPropsDataGenerator(
				_FilteringService, DaoFactory, LocalizationService);
		}

		[Test]
		public void ReportAlingNeededCustomPropsInRigthWay()
		{
			var bug = M.TaskType("Bug");
			var feature = M.TaskType("Feature");
			var support = M.TaskType("Support");
			var b1 = M.Task(1, bug, "bug1", executor: CurrentUser);
			M.Task(2, bug, "bug2", executor: CurrentUser);
			var f1 = M.Task(3, feature, "feature1", executor: CurrentUser);
			var f2 = M.Task(4, feature, "feature2", executor: CurrentUser);
			M.Task(5, support, "support1", executor: CurrentUser);
			var s2 = M.Task(6, support, "support2", executor: CurrentUser);
			var abc = M.ParamType("abc");
			var def = M.ParamType("def");
			var ghi = M.ParamType("ghi");
			M.Param(b1, abc, "111");
			M.Param(b1, ghi, "222");
			M.Param(f1, def, "333");
			M.Param(f2, ghi, "444");
			M.Param(s2, abc, "555");

			/* expect
			 * num | ... | abc | ghi | def
			 * t0-1        111   222
			 * t0-2
			 * t0-3                    333
			 * t0-4              444
			 * t0-5
			 * t0-6       555
			 */

			var gen = MakePropsDataGenerator();
			var doc = gen.GetReportData(MakeParameters(), token);

// ReSharper disable PossibleNullReferenceException
			Assert.That(doc, Is.Not.Null);
			var data = doc.SelectSingleNode("/reportData/range[@name=\"data\"]");
			var headers = doc.SelectSingleNode("/reportData/range[@name=\"paramHeaders\"]");
			Assert.That(data, Is.Not.Null);
			Assert.That(headers, Is.Not.Null);
			var headerRow = headers.SelectSingleNode("item");
			Assert.That(Value(headerRow, "pn1"), Is.EqualTo("abc"));
			Assert.That(Value(headerRow, "pn2"), Is.EqualTo("ghi"));
			Assert.That(Value(headerRow, "pn3"), Is.EqualTo("def"));

			var items = data.SelectNodes("item").OfType<XmlNode>().Reverse().ToArray();
			
			Assert.That(Value(items[0], "p1"), Is.EqualTo("111"));
			Assert.That(Value(items[0], "p2"), Is.EqualTo("222"));
			Assert.That(items[0].SelectSingleNode("value[@name=\"p3\"]"), Is.Null);

			Assert.That(items[1].SelectSingleNode("value[@name=\"p1\"]"), Is.Null);
			Assert.That(items[1].SelectSingleNode("value[@name=\"p2\"]"), Is.Null);
			Assert.That(items[1].SelectSingleNode("value[@name=\"p3\"]"), Is.Null);

			Assert.That(items[2].SelectSingleNode("value[@name=\"p1\"]"), Is.Null);
			Assert.That(items[2].SelectSingleNode("value[@name=\"p2\"]"), Is.Null);
			Assert.That(Value(items[2], "p3"), Is.EqualTo("333"));

			Assert.That(items[3].SelectSingleNode("value[@name=\"p1\"]"), Is.Null);
			Assert.That(Value(items[3], "p2"), Is.EqualTo("444"));
			Assert.That(items[3].SelectSingleNode("value[@name=\"p3\"]"), Is.Null);

			Assert.That(items[4].SelectSingleNode("value[@name=\"p1\"]"), Is.Null);
			Assert.That(items[4].SelectSingleNode("value[@name=\"p2\"]"), Is.Null);
			Assert.That(items[4].SelectSingleNode("value[@name=\"p3\"]"), Is.Null);

			Assert.That(Value(items[5], "p1"), Is.EqualTo("555"));
			Assert.That(items[5].SelectSingleNode("value[@name=\"p2\"]"), Is.Null);
			Assert.That(items[5].SelectSingleNode("value[@name=\"p3\"]"), Is.Null);
// ReSharper restore PossibleNullReferenceException
		}
	}
}
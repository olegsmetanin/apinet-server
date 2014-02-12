using System.Linq;
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
	/// Тесты генераторово данных для отчетов-списков задач
	/// </summary>
	[TestFixture]
	public class TaskListReportsTest: AbstractTest
	{
//		private ProjectParticipantModel pIvanov;
//		private ProjectParticipantModel pPetrov;
		private System.Threading.CancellationToken token;

		[TestFixtureSetUp]
		public new void Init()
		{
			base.Init();

//			var project = Session.QueryOver<ProjectModel>().Where(m => m.ProjectCode == TestProject).SingleOrDefault();
//			Assert.IsNotNull(project);
//			var ivanov = Session.QueryOver<UserModel>().Where(m => m.Login == "user1@apinet-test.com").SingleOrDefault();
//			Assert.IsNotNull(ivanov);
//			var petrov = Session.QueryOver<UserModel>().Where(m => m.Login == "user2@apinet-test.com").SingleOrDefault();
//			Assert.IsNotNull(petrov);
//
//			pIvanov = new ProjectParticipantModel
//			{
//				Project = project,
//				User = ivanov,
//				GroupName = "Executors",
//				IsDefaultGroup = true
//			};
//			_CrudDao.Store(pIvanov);
//			pPetrov = new ProjectParticipantModel
//			{
//				Project = project,
//				User = petrov,
//				GroupName = "Executors",
//				IsDefaultGroup = true
//			};
//			_CrudDao.Store(pPetrov);
//			project.Participants.Add(pIvanov);
//			project.Participants.Add(pPetrov);
//
//			_SessionProvider.FlushCurrentSession();

			var cts = new System.Threading.CancellationTokenSource();
			token = cts.Token;
		}

		[TestFixtureTearDown]
		public new void Cleanup()
		{
			base.Cleanup();
		}

		[SetUp]
		public void SetUp()
		{
		}

		[TearDown]
		public new void TearDown()
		{
			base.TearDown();
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

		private SimpleTaskListDataGenerator MakeSimpleGenerator()
		{
			return new SimpleTaskListDataGenerator(
				IocContainer.GetInstance<IFilteringService>(), IocContainer.GetInstance<IFilteringDao>());
		}

		[Test]
		public void ReportWithDefaultParamsReturnAllRecords()
		{
			var tt1 = M.TaskType("aaa");
			var tt2 = M.TaskType("bbb");
			var t1 = M.Task(1, tt1);
			var t2 = M.Task(2, tt2);
			_SessionProvider.FlushCurrentSession();

			var gen = MakeSimpleGenerator();

			var doc = gen.GetReportData(MakeParameters(), token);

			Assert.IsNotNull(doc);
			var range = doc.SelectSingleNode("/reportData/range[@name=\"data\"]");
			Assert.IsNotNull(range);
			var items = range.SelectNodes("item");
			Assert.IsNotNull(items);
			Assert.AreEqual(2, items.Count);

			t1 = Session.Load<TaskModel>(t1.Id);
			t2 = Session.Load<TaskModel>(t2.Id);

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
	}
}
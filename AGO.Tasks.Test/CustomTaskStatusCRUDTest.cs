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
	/// Тесты CRUD справочника типов статусов
	/// </summary>
	[TestFixture]
	public class CustomTaskStatusCRUDTest: AbstractDictionaryTest
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

		private CustomTaskStatusModel[] MakeSeveral(params string[] names)
		{
			var result = new CustomTaskStatusModel[names.Length];
			for (int i = 0; i < names.Length; i++)
			{
				result[i] = M.CustomStatus(names[i]);
			}
			return result;
		}

		
		//lookup
		[Test]
		public void LookupWithoutTermReturnAllRecords()
		{
			MakeSeveral("s1", "s2", "s3");
			_SessionProvider.FlushCurrentSession();

			//assume ordered result
			var items = Controller.LookupCustomStatuses(TestProject, null, 0).ToArray();

			Assert.AreEqual(3, items.Length);
			Assert.AreEqual("s1", items[0].Text);
			Assert.AreEqual("s2", items[1].Text);
			Assert.AreEqual("s3", items[2].Text);
		}

		[Test]
		public void LookupWithTermReturnMatchedRecords()
		{
			MakeSeveral("s1", "s2", "a3");
			_SessionProvider.FlushCurrentSession();

			//assume ordered result
			var items = Controller.LookupCustomStatuses(TestProject, "s", 0).ToArray();

			Assert.AreEqual(2, items.Length);
			Assert.AreEqual("s1", items[0].Text);
			Assert.AreEqual("s2", items[1].Text);
		}

		[Test]
		public void LookupInNotExistingProjectReturnEmpty()
		{
			MakeSeveral("s1", "s2", "s3");
			_SessionProvider.FlushCurrentSession();

			var items = Controller.LookupCustomStatuses("asdfgh", "s", 0).ToArray();

			Assert.AreEqual(0, items.Length);
		}


		//get
		[Test]
		public void GetStatusesReturnAllRecords()
		{
			var ss = MakeSeveral("s1", "s2", "s3");
			_SessionProvider.FlushCurrentSession();

			var items = Controller.GetCustomStatuses(TestProject, 
				Enumerable.Empty<IModelFilterNode>().ToArray(), 
				new []{new SortInfo{ Property = "Name"}},
				0).ToArray();

			Assert.AreEqual(3, items.Length);
			Assert.AreEqual(ss[0].Id, items[0].Id);
			Assert.AreEqual(ss[0].Name, items[0].Name);
			Assert.AreEqual(ss[1].Id, items[1].Id);
			Assert.AreEqual(ss[1].Name, items[1].Name);
			Assert.AreEqual(ss[2].Id, items[2].Id);
			Assert.AreEqual(ss[2].Name, items[2].Name);
		}

		[Test]
		public void GetStatusesWithoutProjectReturnEmpty()
		{
			MakeSeveral("s1", "s2", "s3");
			_SessionProvider.FlushCurrentSession();

			var items = Controller.GetCustomStatuses("asdfgh",
				Enumerable.Empty<IModelFilterNode>().ToArray(),
				new[] { new SortInfo { Property = "Name" } },
				0).ToArray();

			Assert.AreEqual(0, items.Length);
		}

		[Test]
		public void GetStatusesWithFilterReturnMatchedRecords()
		{
			var ss = MakeSeveral("s1", "s2", "s3");
			_SessionProvider.FlushCurrentSession();

			var predicate = _FilteringService
				.Filter<CustomTaskStatusModel>()
				.Or()
				.WhereString(m => m.Name).Like("1")
				.WhereString(m => m.Name).Like("3");

			var items = Controller.GetCustomStatuses(TestProject,
				new [] { predicate },
				new[] { new SortInfo { Property = "Name" } },
				0).ToArray();

			Assert.AreEqual(2, items.Length);
			Assert.AreEqual(ss[0].Id, items[0].Id);
			Assert.AreEqual(ss[0].Name, items[0].Name);
			Assert.AreEqual(ss[2].Id, items[1].Id);
			Assert.AreEqual(ss[2].Name, items[1].Name);
		}


		//edit
		[Test]
		public void CreateValidStatusReturnSuccess()
		{
			var model = new CustomStatusDTO {Name = "new", ViewOrder = 1};

			var vr = Controller.EditCustomStatus(TestProject, model);
			_SessionProvider.FlushCurrentSession(!vr.Success);

			Assert.IsTrue(vr.Success);
		}

		[Test]
		public void CreateStatusWithoutNameReturnError()
		{
			var model = new CustomStatusDTO { Name = string.Empty, ViewOrder = 1 };

			var vr = Controller.EditCustomStatus(TestProject, model);
			_SessionProvider.FlushCurrentSession(!vr.Success);

			Assert.IsFalse(vr.Success);
		}

		[Test]
		public void UpdateValidStatusReturnSuccess()
		{
			var s = M.CustomStatus();
			_SessionProvider.FlushCurrentSession();
			var model = new CustomStatusDTO {Id = s.Id, Name = "newName", ViewOrder = s.ViewOrder};

			var vr = Controller.EditCustomStatus(TestProject, model);
			_SessionProvider.FlushCurrentSession(!vr.Success);

			Assert.IsTrue(vr.Success);
			s = Session.Get<CustomTaskStatusModel>(s.Id);
			Assert.IsNotNull(s);
			Assert.AreEqual("newName", s.Name);
		}

		[Test]
		public void UpdateStatusWithoutNameReturnError()
		{
			var s = M.CustomStatus();
			_SessionProvider.FlushCurrentSession();
			var model = new CustomStatusDTO { Id = s.Id, Name = string.Empty, ViewOrder = s.ViewOrder };

			var vr = Controller.EditCustomStatus(TestProject, model);
			_SessionProvider.FlushCurrentSession(!vr.Success);

			Assert.IsFalse(vr.Success);
			s = Session.Get<CustomTaskStatusModel>(s.Id);
			Assert.IsNotNull(s);
			Assert.AreEqual("status", s.Name);
		}
		

		//delete
		[Test]
		public void DeleteStatusWithoutRefsReturnSuccess()
		{
			var s = M.CustomStatus();
			_SessionProvider.FlushCurrentSession();

			var result = Controller.DeleteCustomStatus(s.Id);
			_SessionProvider.FlushCurrentSession(!result);

			Assert.IsTrue(result);
			s = Session.Get<CustomTaskStatusModel>(s.Id);
			Assert.IsNull(s);
		}

		[Test, ExpectedException(typeof(CannotDeleteReferencedItemException))]
		public void DeleteStatusWithRefsThrow()
		{
			var s = M.CustomStatus();
			var task = M.Task(1);
			task.ChangeCustomStatus(s, CurrentUser);
			_SessionProvider.FlushCurrentSession();

			var result = Controller.DeleteCustomStatus(s.Id);
			_SessionProvider.FlushCurrentSession(!result);

			Assert.IsTrue(result);
			s = Session.Get<CustomTaskStatusModel>(s.Id);
			Assert.IsNull(s);
		}

		[Test]
		public void DeleteTaskStatusesWithoutRefsReturnSuccess()
		{
			var ss = MakeSeveral("s1", "s2");
			_SessionProvider.FlushCurrentSession();

			var result = Controller.DeleteCustomStatuses(TestProject, new[] {ss[0].Id, ss[1].Id}, null);
			_SessionProvider.FlushCurrentSession(!result);

			Assert.IsTrue(result);
			ss[0] = Session.Get<CustomTaskStatusModel>(ss[0].Id);
			ss[1] = Session.Get<CustomTaskStatusModel>(ss[1].Id);
			Assert.IsNull(ss[0]);
			Assert.IsNull(ss[1]);
		}

		[Test, ExpectedException(typeof(CannotDeleteReferencedItemException))]
		public void DeleteTaskStatusesWithRefsThrow()
		{
			var s = M.CustomStatus();
			var task = M.Task(1);
			task.ChangeCustomStatus(s, CurrentUser);
			_SessionProvider.FlushCurrentSession();

			Controller.DeleteCustomStatuses(TestProject, new[] { s.Id }, null);
		}

		[Test]
		public void DeleteTaskStatusesWithRefsAndReplacementReturnSuccess()
		{
			var s1 = M.CustomStatus("duplicate");
			var s2 = M.CustomStatus("replacement");
			var task = M.Task(1);
			task.ChangeCustomStatus(s1, CurrentUser);
			_SessionProvider.FlushCurrentSession();

			var result = Controller.DeleteCustomStatuses(TestProject, new[] { s1.Id }, s2.Id);
			_SessionProvider.FlushCurrentSession(!result);

			Assert.IsTrue(result);
			s1 = Session.Get<CustomTaskStatusModel>(s1.Id);
			s2 = Session.Get<CustomTaskStatusModel>(s2.Id);
			Assert.IsNull(s1);
			Assert.IsNotNull(s2);
			task = Session.Get<TaskModel>(task.Id);
			Assert.AreEqual(s2.Id, task.CustomStatus.Id);
			Assert.AreEqual(3, task.ModelVersion);
			var history = task.CustomStatusHistory.First();
			Assert.AreEqual(s2.Id, history.Status.Id);
			Assert.AreEqual(2, history.ModelVersion);
		}
	}
}
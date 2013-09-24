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
	/// Тесты CRUD справочника типов статусов
	/// </summary>
	[TestFixture]
	public class CustomTaskStatusCRUDTest: AbstractDictionaryTest
	{
		[SetUp]
		public new void Init()
		{
			base.Init();
		}

		[TearDown]
		public new void Cleanup()
		{
			base.Cleanup();
		}

		private CustomTaskStatusModel Make(string name)
		{
			var m = new CustomTaskStatusModel
			        	{
			        		ProjectCode = TestProject,
			        		Name = name
			        	};
			Session.Save(m);
			return m;
		}


		//lookup
		[Test]
		public void LookupWithoutTermReturnAllRecords()
		{
			Make("s1");
			Make("s2");
			Make("s3");
			_SessionProvider.CloseCurrentSession();

			//assume ordered result
			var items = Controller.LookupCustomStatuses(TestProject, null, 0, 10).ToArray();

			Assert.AreEqual(3, items.Length);
			Assert.AreEqual("s1", items[0].Text);
			Assert.AreEqual("s2", items[1].Text);
			Assert.AreEqual("s3", items[2].Text);
		}

		[Test]
		public void LookupWithTermReturnMatchedRecords()
		{
			Make("s1");
			Make("s2");
			Make("a3");
			_SessionProvider.CloseCurrentSession();

			//assume ordered result
			var items = Controller.LookupCustomStatuses(TestProject, "s", 0, 10).ToArray();

			Assert.AreEqual(2, items.Length);
			Assert.AreEqual("s1", items[0].Text);
			Assert.AreEqual("s2", items[1].Text);
		}

		[Test]
		public void LookupInNotExistingProjectReturnEmpty()
		{
			Make("s1");
			Make("s2");
			Make("s3");
			_SessionProvider.CloseCurrentSession();

			var items = Controller.LookupCustomStatuses("asdfgh", "s", 0, 10).ToArray();

			Assert.AreEqual(0, items.Length);
		}


		//get
		[Test]
		public void GetStatusesReturnAllRecords()
		{
			var m1 = Make("s1");
			var m2 = Make("s2");
			var m3 = Make("s3");
			_SessionProvider.CloseCurrentSession();

			var items = Controller.GetCustomStatuses(TestProject, 
				Enumerable.Empty<IModelFilterNode>().ToArray(), 
				new []{new SortInfo{ Property = "Name"}},
				0, 10).ToArray();

			Assert.AreEqual(3, items.Length);
			Assert.AreEqual(m1.Id, items[0].Id);
			Assert.AreEqual(m1.Name, items[0].Name);
			Assert.AreEqual(m2.Id, items[1].Id);
			Assert.AreEqual(m2.Name, items[1].Name);
			Assert.AreEqual(m3.Id, items[2].Id);
			Assert.AreEqual(m3.Name, items[2].Name);
		}

		[Test]
		public void GetStatusesWithoutProjectReturnEmpty()
		{
			Make("s1");
			Make("s2");
			Make("s3");
			_SessionProvider.CloseCurrentSession();

			var items = Controller.GetCustomStatuses("asdfgh",
				Enumerable.Empty<IModelFilterNode>().ToArray(),
				new[] { new SortInfo { Property = "Name" } },
				0, 10).ToArray();

			Assert.AreEqual(0, items.Length);
		}

		[Test]
		public void GetStatusesWithFilterReturnMatchedRecords()
		{
			var m1 = Make("s1");
			Make("s2");
			var m3 = Make("s3");
			_SessionProvider.CloseCurrentSession();

			var predicate = _FilteringService
				.Filter<CustomTaskStatusModel>()
				.Or()
				.WhereString(m => m.Name).Like("1")
				.WhereString(m => m.Name).Like("3");

			var items = Controller.GetCustomStatuses(TestProject,
				new [] { predicate },
				new[] { new SortInfo { Property = "Name" } },
				0, 10).ToArray();

			Assert.AreEqual(2, items.Length);
			Assert.AreEqual(m1.Id, items[0].Id);
			Assert.AreEqual(m1.Name, items[0].Name);
			Assert.AreEqual(m3.Id, items[1].Id);
			Assert.AreEqual(m3.Name, items[1].Name);
		}


		//edit
		[Test]
		public void CreateValidStatusReturnSuccess()
		{
			var model = new CustomStatusDTO {Name = "new", ViewOrder = 1};

			var vr = Controller.EditCustomStatus(TestProject, model);
			_SessionProvider.CloseCurrentSession();

			Assert.IsTrue(vr.Success);
		}

		[Test]
		public void CreateStatusWithoutNameReturnError()
		{
			var model = new CustomStatusDTO { Name = string.Empty, ViewOrder = 1 };

			var vr = Controller.EditCustomStatus(TestProject, model);
			_SessionProvider.CloseCurrentSession();

			Assert.IsFalse(vr.Success);
		}

		[Test]
		public void UpdateValidStatusReturnSuccess()
		{
			var s = Make("status");
			_SessionProvider.CloseCurrentSession();
			var model = new CustomStatusDTO {Id = s.Id, Name = "newName", ViewOrder = s.ViewOrder};

			var vr = Controller.EditCustomStatus(TestProject, model);
			_SessionProvider.CloseCurrentSession();

			Assert.IsTrue(vr.Success);
			s = Session.Get<CustomTaskStatusModel>(s.Id);
			Assert.IsNotNull(s);
			Assert.AreEqual("newName", s.Name);
		}

		[Test]
		public void UpdateStatusWithoutNameReturnError()
		{
			var s = Make("status");
			_SessionProvider.CloseCurrentSession();
			var model = new CustomStatusDTO { Id = s.Id, Name = string.Empty, ViewOrder = s.ViewOrder };

			var vr = Controller.EditCustomStatus(TestProject, model);
			_SessionProvider.CloseCurrentSession();

			Assert.IsFalse(vr.Success);
			s = Session.Get<CustomTaskStatusModel>(s.Id);
			Assert.IsNotNull(s);
			Assert.AreEqual("status", s.Name);
		}
		

		//delete
		[Test]
		public void DeleteStatusWithoutRefsReturnSuccess()
		{
			var s = Make("status");
			_SessionProvider.CloseCurrentSession();

			var result = Controller.DeleteCustomStatus(s.Id);
			_SessionProvider.CloseCurrentSession();

			Assert.IsTrue(result);
			s = Session.Get<CustomTaskStatusModel>(s.Id);
			Assert.IsNull(s);
		}

		[Test, ExpectedException(typeof(CannotDeleteReferencedItemException))]
		public void DeleteStatusWithRefsThrow()
		{
			var s = Make("status");
			var tt = new TaskTypeModel {ProjectCode = TestProject, Name = "tt"};
			Session.Save(tt);
			var task = new TaskModel
			           	{
			           		ProjectCode = TestProject, 
							CustomStatus = s,
							SeqNumber = "1",
							InternalSeqNumber = 1,
							TaskType = tt
			           	};
			Session.Save(task);
			var history = new CustomTaskStatusHistoryModel {Task = task, Status = s, Start = DateTime.Now};
			Session.Save(history);
			_SessionProvider.CloseCurrentSession();

			var result = Controller.DeleteCustomStatus(s.Id);
			_SessionProvider.CloseCurrentSession();

			Assert.IsTrue(result);
			s = Session.Get<CustomTaskStatusModel>(s.Id);
			Assert.IsNull(s);
		}

		[Test]
		public void DeleteTaskStatusesWithoutRefsReturnSuccess()
		{
			var s1 = Make("s1");
			var s2 = Make("s2");
			_SessionProvider.CloseCurrentSession();

			var result = Controller.DeleteCustomStatuses(TestProject, new[] {s1.Id, s2.Id}, null);
			_SessionProvider.CloseCurrentSession();

			Assert.IsTrue(result);
			s1 = Session.Get<CustomTaskStatusModel>(s1.Id);
			s2 = Session.Get<CustomTaskStatusModel>(s2.Id);
			Assert.IsNull(s1);
			Assert.IsNull(s2);
		}

		[Test, ExpectedException(typeof(CannotDeleteReferencedItemException))]
		public void DeleteTaskStatusesWithRefsThrow()
		{
			var s = Make("s");
			var tt = new TaskTypeModel { ProjectCode = TestProject, Name = "tt" };
			Session.Save(tt);
			var task = new TaskModel
			{
				ProjectCode = TestProject,
				CustomStatus = s,
				SeqNumber = "1",
				InternalSeqNumber = 1,
				TaskType = tt
			};
			Session.Save(task);
			_SessionProvider.CloseCurrentSession();

			Controller.DeleteCustomStatuses(TestProject, new[] { s.Id }, null);
		}

		[Test]
		public void DeleteTaskStatusesWithRefsAndReplacementReturnSuccess()
		{
			var s1 = Make("s1");
			var s2 = Make("s2");
			var tt = new TaskTypeModel { ProjectCode = TestProject, Name = "tt" };
			Session.Save(tt);
			var task = new TaskModel
			{
				ProjectCode = TestProject,
				CustomStatus = s1,
				SeqNumber = "1",
				InternalSeqNumber = 1,
				TaskType = tt
			};
			Session.Save(task);
			var history = new CustomTaskStatusHistoryModel { Task = task, Status = s1, Start = DateTime.Now };
			Session.Save(history);
			_SessionProvider.CloseCurrentSession();

			var result = Controller.DeleteCustomStatuses(TestProject, new[] { s1.Id }, s2.Id);
			_SessionProvider.CloseCurrentSession();

			Assert.IsTrue(result);
			s1 = Session.Get<CustomTaskStatusModel>(s1.Id);
			s2 = Session.Get<CustomTaskStatusModel>(s2.Id);
			Assert.IsNull(s1);
			Assert.IsNotNull(s2);
			task = Session.Get<TaskModel>(task.Id);
			Assert.AreEqual(s2.Id, task.CustomStatus.Id);
			Assert.AreEqual(2, task.ModelVersion);
			history = Session.Get<CustomTaskStatusHistoryModel>(history.Id);
			Assert.AreEqual(s2.Id, history.Status.Id);
			Assert.AreEqual(2, history.ModelVersion);
		}
	}
}
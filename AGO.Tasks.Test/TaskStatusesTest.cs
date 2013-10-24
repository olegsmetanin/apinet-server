using System.Globalization;
using System.Linq;
using AGO.Tasks.Model.Task;
using NUnit.Framework;
using Sys = System;

namespace AGO.Tasks.Test
{
	/// <summary>
	/// Тесты cRud статусов задач (только R, т.к. набор фиксированный)
	/// </summary>
	public class TaskStatusesTest: AbstractDictionaryTest
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
		public void LookupTaskStatusesWithoutTermReturnAll()
		{
			Sys.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
			var result = Controller.LookupTaskStatuses(null, 0).ToArray();

			Assert.AreEqual(5, result.Length);
			Assert.AreEqual(TaskStatus.NotStarted.ToString(), result[0].Id);
			Assert.AreEqual(TaskStatus.InWork.ToString(), result[1].Id);
			Assert.AreEqual(TaskStatus.Completed.ToString(), result[2].Id);
			Assert.AreEqual(TaskStatus.Closed.ToString(), result[3].Id);
			Assert.AreEqual(TaskStatus.Suspended.ToString(), result[4].Id);
		}

		[Test]
		public void LookupTaskStatusesFilterByTerm()
		{
			Sys.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru");
			var result = Controller.LookupTaskStatuses("та", 0).ToArray();

			Assert.AreEqual(2, result.Length);
			Assert.AreEqual(TaskStatus.Closed.ToString(), result[0].Id); //ЗакрыТА
			Assert.AreEqual(TaskStatus.Suspended.ToString(), result[1].Id); //ПриосТАновлена
		}
		 
	}
}
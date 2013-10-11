using System.Linq;
using AGO.Tasks.Model.Task;
using NUnit.Framework;

namespace AGO.Tasks.Test
{
	/// <summary>
	/// Тесты cRud приоритетов задач (только R, т.к. набор фиксированный)
	/// </summary>
	[TestFixture]
	public class TaskPrioritiesTest : AbstractDictionaryTest
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
		public void LookupTaskPrioritiesWithoutTermReturnAll()
		{
			var result = Controller.LookupTaskPriorities(null, 0).ToArray();

			Assert.AreEqual(3, result.Length);
			Assert.AreEqual(TaskPriority.Low.ToString(), result[0].Id);
			Assert.AreEqual(TaskPriority.Normal.ToString(), result[1].Id);
			Assert.AreEqual(TaskPriority.High.ToString(), result[2].Id);
		}

		[Test]
		public void LookupTaskPrioritiesFilterByTerm()
		{
			var result = Controller.LookupTaskPriorities("ки", 0).ToArray();

			Assert.AreEqual(2, result.Length);
			Assert.AreEqual(TaskPriority.Low.ToString(), result[0].Id); //НизКИй
			Assert.AreEqual(TaskPriority.High.ToString(), result[1].Id); //ВысоКИй
		}
	}
}
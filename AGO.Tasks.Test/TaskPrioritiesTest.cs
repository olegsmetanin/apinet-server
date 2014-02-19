using System.Globalization;
using System.Linq;
using AGO.Tasks.Model.Task;
using NUnit.Framework;
using Sys = System;

namespace AGO.Tasks.Test
{
	/// <summary>
	/// Тесты cRud приоритетов задач (только R, т.к. набор фиксированный)
	/// </summary>
	public class TaskPrioritiesTest : AbstractDictionaryTest
	{
		[Test]
		public void LookupTaskPrioritiesWithoutTermReturnAll()
		{
			Sys.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
			var result = Controller.LookupTaskPriorities(null, 0).ToArray();

			Assert.AreEqual(3, result.Length);
			Assert.AreEqual(TaskPriority.Low.ToString(), result[0].Id);
			Assert.AreEqual(TaskPriority.Normal.ToString(), result[1].Id);
			Assert.AreEqual(TaskPriority.High.ToString(), result[2].Id);
		}

		[Test]
		public void LookupTaskPrioritiesFilterByTerm()
		{
			Sys.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru");
			var result = Controller.LookupTaskPriorities("ки", 0).ToArray();

			Assert.AreEqual(2, result.Length);
			Assert.AreEqual(TaskPriority.Low.ToString(), result[0].Id); //НизКИй
			Assert.AreEqual(TaskPriority.High.ToString(), result[1].Id); //ВысоКИй
		}
	}
}
using NUnit.Framework;

namespace AGO.WorkQueue.Tests
{
	/// <summary>
	/// Тесты базовых сценариев использования очереди задач
	/// </summary>
	[TestFixture]
	public class InMemoryQueueConsumptionTest : AbstractQueueConsumptionTest
	{
		protected override IWorkQueue CreateQueue()
		{
			return new InMemoryWorkQueue();
		}
	}
}

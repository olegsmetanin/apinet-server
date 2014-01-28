using NUnit.Framework;

namespace AGO.WorkQueue.Tests
{
	[TestFixture]
	public class InMemoryQueueParallelUsageTest : AbstractQueueParallelUsageTest
	{
		protected override IWorkQueue CreateQueue()
		{
			return new InMemoryWorkQueue();
		}
	}
}
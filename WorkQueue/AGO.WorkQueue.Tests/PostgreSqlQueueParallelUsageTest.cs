using NUnit.Framework;

namespace AGO.WorkQueue.Tests
{
	[TestFixture]
	public class PostgreSqlQueueParallelUsageTest: AbstractQueueParallelUsageTest
	{
		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			PostgreSqlHelper.CreateDbAndSchema();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			PostgreSqlHelper.DropDb();
		}

		[TearDown]
		public void TearDown()
		{
			PostgreSqlHelper.ExecuteBatch(PostgreSqlHelper.ConnStr, "truncate work_queue");
		}

		protected override IWorkQueue CreateQueue()
		{
			return new PostgreSqlWorkQueue(PostgreSqlHelper.ConnStr);
		}
	}
}
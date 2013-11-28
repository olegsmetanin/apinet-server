using System.Linq;
using System.Text;
using AGO.Core.Model.Reporting;
using AGO.Reporting.Common;
using NHibernate;
using NUnit.Framework;

namespace AGO.Reporting.Tests
{
	[TestFixture]
	public class ReportingRepositoryTest: AbstractReportingTest
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

		private IReportingRepository repo;

		[SetUp]
		public void SetUp()
		{
			repo = IocContainer.GetInstance<IReportingRepository>();
		}

		[TearDown]
		public new void TearDown()
		{
			repo = null;
			base.TearDown();
		}

		[Test]
		public void GetAllSettings()
		{
			var descriptors = repo.GetAllDescriptors().ToList();

			Assert.IsNotNull(descriptors);
			Assert.AreEqual(2, descriptors.Count);
		}

		[Test]
		public void GetInstanceSetting()
		{
			//see base class
			const string instanceName = "NUnit Fast reports";
			
			var instanceDescriptor = repo.GetDescriptor(instanceName);

			Assert.IsNotNull(instanceDescriptor);
			Assert.AreEqual(instanceName, instanceDescriptor.Name);
			Assert.AreEqual("http://localhost:36652/api", instanceDescriptor.EndPoint);
			Assert.IsFalse(instanceDescriptor.LongRunning);
		}

//		[Test]
//		public void GetTask()
//		{
//			var res = new byte[1024*1024 * 5];
//			var tpl = M.Template("my template", Encoding.UTF8.GetBytes("aaa bbb"));
//			var setting = M.Setting("my setting", tpl.Id);
//			var task = M.Task("my task", "Fast reports", setting.Id);
//			task.Result = res;
//			Session.SaveOrUpdate(task);
//			Session.FlushMode = FlushMode.Auto;
//			_SessionProvider.FlushCurrentSession();
//
//			task = Session.Get<ReportTaskModel>(task.Id);
//
//			Assert.IsNotNull(tpl);
//			Assert.AreEqual("NUnit my task", task.Name);
//			Assert.AreEqual("NUnit Fast reports", task.Service.Name);
//			Assert.AreEqual(setting.Id, task.Setting.Id);
//			CollectionAssert.AreEqual(res, task.Result);
//		}
	}
}
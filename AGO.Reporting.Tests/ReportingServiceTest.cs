using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using AGO.Core.Config;
using AGO.Core.Model.Reporting;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;
using AGO.Reporting.Service;
using NHibernate;
using NUnit.Framework;
using Newtonsoft.Json;

namespace AGO.Reporting.Tests
{
	[TestFixture]
	public class ReportingServiceTest: AbstractReportingTest
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

		private IReportingService svc;

		[SetUp]
		public void SetUp()
		{
			var realsvc = new ReportingService();
			realsvc.KeyValueProvider = new AppSettingsKeyValueProvider();
			realsvc.Initialize();
			svc = realsvc;
		}

		[TearDown]
		public new void TearDown()
		{
			svc.Dispose();
			svc = null;
			base.TearDown();
		}

		[Test]
		public void PingAlwaysReturnTrue()
		{
			Assert.IsTrue(svc.Ping());
		}

		[Test]
		public void GenerateCsvReport()
		{
			var tpl = M.Template("fake", Encoding.UTF8.GetBytes("<range_data>\"{$num$}\",\"{$txt$}\"</range_data>"));
			var stg = M.Setting("fake", tpl.Id, GeneratorType.CvsGenerator, 
				typeof (FakeGenerator).AssemblyQualifiedName,
				typeof(FakeParameters).AssemblyQualifiedName);
			var task = M.Task("controlled", "Fast reports", stg.Id, "{a:1, b:'zxc'}");
			_SessionProvider.FlushCurrentSession();

			svc.RunReport(task.Id);

			var safeCounter = 0;
			while (safeCounter < 10)
			{
				Thread.Sleep(500);
				safeCounter++;
				Session.Refresh(task);
				if (task.State == ReportTaskState.Completed) break;
			}

			Assert.AreEqual(ReportTaskState.Completed, task.State);
			Assert.IsNotNull(task.StartedAt);
			Assert.IsNotNull(task.CompletedAt);
			Assert.Greater(task.CompletedAt.Value, task.StartedAt.Value);
			StringAssert.AreEqualIgnoringCase("\"1\",\"zxc\"", Encoding.UTF8.GetString(task.Result));
		}
	}

	public class FakeParameters
	{
		[JsonProperty("a")]
		public int A { get; set; }

		[JsonProperty("b")]
		public string B { get; set; }
	}

	public class FakeGenerator: BaseReportDataGenerator
	{
		protected override void FillReportData(object parameters)
		{
			var p = (FakeParameters)parameters;
			var range = MakeRange("data", string.Empty);
			var item = MakeItem(range);
			MakeValue(item, "num", p.A.ToString(CultureInfo.CurrentUICulture));
			MakeValue(item, "txt", p.B, false);
		}
	}
}
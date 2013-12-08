using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AGO.Reporting.Tests
{
	[TestFixture]
	public class TmpTest
	{
		 

		[Test]
		public void Cancel()
		{
			var s = new CancellationTokenSource();
			var task = Task.Factory.StartNew(() => LoopWithCheck(s.Token), s.Token);
			Thread.Sleep(300);

			s.Cancel();
			Thread.Sleep(150);

			Assert.AreEqual(TaskStatus.Canceled, task.Status);

			try
			{
				task.Wait();
			}
			catch (Exception e)
			{
				var aex = e as AggregateException;
				Assert.IsNotNull(aex);
				Assert.AreEqual(typeof(TaskCanceledException), aex.InnerExceptions[0].GetType());
			}
		}

		[Test]
		public void Abort()
		{
			var s = new CancellationTokenSource();
			s.Token.Register(Thread.CurrentThread.Abort);
			var task = Task.Factory.StartNew(LoopWithoutCheck, s.Token);
			Thread.Sleep(300);

			try
			{
				s.Cancel();
				Thread.Sleep(150);

				Assert.AreEqual(TaskStatus.Canceled, task.Status);
				//Assert.AreEqual(typeof(OperationCanceledException), task.Exception.InnerExceptions[0].GetType());

				task.Wait();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private void LoopWithoutCheck()
		{
			Thread.Sleep(5000);
		}

		private void LoopWithCheck(CancellationToken token)
		{
			for(var i = 0; i < 100; i++)
			{
				Thread.Sleep(100);
				token.ThrowIfCancellationRequested();
			}
		}
	}
}
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AGO.Reporting.Tests
{
	[TestFixture]
	public class TPLManageTest
	{
		private Stopwatch watch;
		private CancellationTokenSource s;
		private bool canceled;
		private bool completed;
		private bool error;
		private bool redirect;

		private bool wasTimedOut;

		[SetUp]
		public void SetUp()
		{
			watch = new Stopwatch();
			s = new CancellationTokenSource();
			canceled = completed = error = redirect = wasTimedOut = false;
		}

		public void TearDown()
		{
			watch.Stop();
			watch = null;
			s.Dispose();
		}

		[Test]
		public void Cancel()
		{
			var tasks = Start(LoopWithCheck, 2000);
			watch.Start();
			tasks[0].Start();
			Thread.Sleep(300);

			s.Cancel();
			Thread.Sleep(150);
			Assert.IsTrue(canceled);
			Assert.IsFalse(completed);
			Assert.IsFalse(error);
			Assert.IsFalse(redirect);

			Wait(tasks);
			Log("Finish test: Cancel");
		}

		[Test]
		public void CancelByTimeout()
		{
			var tasks = Start(LoopWithCheck, 400);
			watch.Start();
			tasks[0].Start();

			Thread.Sleep(450);

			Assert.IsTrue(canceled);
			Assert.IsFalse(completed);
			Assert.IsFalse(error);
			Assert.IsTrue(redirect);

			Wait(tasks);
			Log("Finish test: CancelByTimeout");
		}

		[Test]
		public void Abort()
		{
			var tasks = Start(LoopWithoutCheck, 500);
			watch.Start();
			tasks[0].Start();
			Thread.Sleep(300);

			s.Cancel();
			Thread.Sleep(150);
			Assert.IsTrue(canceled);
			Assert.IsFalse(completed);
			Assert.IsFalse(error);
			Assert.IsFalse(redirect);

			Wait(tasks);
			Log("Finish test: Abort");
		}

		[Test]
		public void AbortByTimeout()
		{
			var tasks = Start(LoopWithoutCheck, 500);
			watch.Start();
			tasks[0].Start();
			
			Thread.Sleep(600);
			Assert.IsTrue(canceled);
			Assert.IsFalse(completed);
			Assert.IsFalse(error);
			Assert.IsTrue(redirect);
			
			Wait(tasks);
			Log("Finish test: AbortByTimeout");//if you want to see last message from abandoned task, break on this line for 5sec, run and see nunit log
		}

		private Task[] Start(Action<CancellationToken> action, int timeout)
		{
			var task = new Task(() => LoopWrapper(action, timeout), s.Token);
			var whenCompleted = task.ContinueWith(t =>
			                                      	{
			                                      		completed = true;
			                                      		Log("Task completed");
			                                      	}, TaskContinuationOptions.OnlyOnRanToCompletion);
			var whenCancel = task.ContinueWith(t =>
			                                   	{
			                                   		canceled = true;
													if (wasTimedOut)
														redirect = true;
			                                   		Log("Task canceled" + (redirect ? " and redirected" : string.Empty));
			                                   	}, TaskContinuationOptions.OnlyOnCanceled);
			var whenError = task.ContinueWith(t =>
			                                  	{
			                                  		error = true;
			                                  		Log("Task error: {0}", t.Exception != null ? t.Exception.ToString() : "no exception");
			                                  	}, TaskContinuationOptions.OnlyOnFaulted);

			return new[] {task, whenCompleted, whenCancel, whenError};
		}

		private void Wait(params Task[] tasks)
		{
			try
			{
				Task.WaitAll(tasks);
			}
			catch (Exception e)
			{
				var aex = e as AggregateException;
				Assert.IsNotNull(aex);
				Assert.AreEqual(typeof(TaskCanceledException), aex.InnerExceptions[0].GetType());
			}
		}

		private void LoopWrapper(Action<CancellationToken> action, int timeout)
		{
			Log("LoopWrapper: started");
			var innerTask = Task.Factory.StartNew(() => action(s.Token), s.Token);
			try
			{
				var result = innerTask.Wait(timeout, s.Token);
				if (result)
				{
					Log("LoopWrapper completed");
				}
				else
				{
					Log("LoopWrapper wait timeout, do cancelation");
					wasTimedOut = true;
					s.Cancel();
					s.Token.ThrowIfCancellationRequested();
				}
			}
			catch (AggregateException aex)
			{
				if (aex.InnerExceptions[0].GetType() != typeof(TaskCanceledException))
					throw;
			}
		}

		private void LoopWithoutCheck(CancellationToken token)
		{
			Log("LoopWithoutCheck: start");
			Thread.Sleep(5000);
			if (token.IsCancellationRequested)
				Log("LoopWithoutCheck: finish in canceled state");
			else
				Log("LoopWithoutCheck: finish");
		}

		private void LoopWithCheck(CancellationToken token)
		{
			for(var i = 0; i < 100; i++)
			{
				Log("LoopWithCheck: {0}", i);
				Thread.Sleep(100);
				if (token.IsCancellationRequested)
					Log("LoopWithCheck: cancel requested");
				token.ThrowIfCancellationRequested();
			}
		}

		private void Log(string fmt, params object[] prms)
		{
			Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " (" + watch.ElapsedMilliseconds + "ms): " + string.Format(fmt, prms));
		}
	}
}
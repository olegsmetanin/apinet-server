using System;
using System.Threading;
using System.Threading.Tasks;
using AGO.Core;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;

namespace AGO.Reporting.Service
{
	/// <summary>
	/// Базовый класс для worker-ов - генераторов отчетов.
	/// </summary>
	public abstract class AbstractReportWorker
	{
		public Guid TaskId { get; set; }

		public TemplateResolver TemplateResolver { get; set; }

		public IReportingRepository Repository { get; set; }

		public ISessionProvider SessionProvider { get; set; }

		public object Parameters { get; set; }

		public bool Finished { get; protected set; }

		public TimeSpan Timeout { get; set; }

		public abstract void Prepare(IReportTask task);

		private Task<IReportGeneratorResult> task;
		protected CancellationTokenSource TokenSource;

		public void Start()
		{
			RegisterStart();
			
			TokenSource = new CancellationTokenSource();
			task = new Task<IReportGeneratorResult> (InternalStart, TokenSource.Token);
			var whenSuccess = task.ContinueWith(t => RegisterSuccessAndSaveResult(t.Result), TaskContinuationOptions.NotOnFaulted);
			var whenCancel = task.ContinueWith(genTask => RegisterCancel(), TaskContinuationOptions.OnlyOnCanceled);
			var whenError = task.ContinueWith(t => 
				{
				    var ex = t.Exception != null && 
						t.Exception.InnerExceptions != null &&
				        t.Exception.InnerExceptions.Count > 0
				            ? t.Exception.InnerExceptions[0]
				            : null;
					RegisterError(ex);
				}, TaskContinuationOptions.OnlyOnFaulted);
			
			Task.Factory.ContinueWhenAny(new[] {whenSuccess, whenError, whenCancel}, t => Finish());
			task.Start();
		}

		private void RegisterStart()
		{
			ChangeState(rt =>
			{
				rt.State = ReportTaskState.Running;
				rt.StartedAt = DateTime.Now;
			});
		}

		private void RegisterSuccessAndSaveResult(IReportGeneratorResult result)
		{
			ChangeState(rt =>
			{
			    rt.State = ReportTaskState.Completed;
			    rt.CompletedAt = DateTime.Now;
			    rt.DateGenerationProgress = 100; //fix generator 
			    rt.ReportGenerationProgress = 100; //ticker errors
			    var buffer = new byte[result.Result.Length];
			    result.Result.Position = 0;
			    result.Result.Read(buffer, 0, buffer.Length);
			    rt.Result = buffer;
			    rt.ResultName = result.FileName ?? rt.Name;
			    rt.ResultContentType = result.ContentType;
			});
		}

		private void RegisterError(Exception ex)
		{
			ChangeState(rt =>
			{
			    rt.State = ReportTaskState.Error;
			    rt.CompletedAt = DateTime.Now;
			    rt.ErrorMsg = ex != null ? ex.Message : "Unknown error";
			    rt.ErrorDetails = ex != null ? ex.ToString() : string.Empty;
			});
		}

		private void RegisterCancel()
		{
			ChangeState(rt =>
			{
			    rt.State = ReportTaskState.Canceled;
			    rt.CompletedAt = DateTime.Now;
			});
		}

		private void Finish()
		{
			//TODO close session, stop timers and so on
			Finished = true;
		}

		protected abstract IReportGeneratorResult InternalStart();

		private readonly object stateChangeSync = new object();
		//т.к. изменение состояния задачи вызывается двумя потоками - основным, делающим работу,
		//и таймером, обновляющим процент выполнения, используем синхронизацию, что бы не получить
		//LockingException на пустом месте
		protected void ChangeState(Action<IReportTask> action)
		{
			lock (stateChangeSync)
			{
				var reportTask = Repository.GetTask(TaskId);
				action(reportTask);
				SessionProvider.CurrentSession.SaveOrUpdate(reportTask);
				SessionProvider.FlushCurrentSession();
			}
		}

	}
}
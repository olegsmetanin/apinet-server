using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AGO.Core;
using AGO.Core.Application;
using AGO.Core.Config;
using AGO.Core.Model.Reporting;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;
using AGO.Reporting.Service.Workers;
using Common.Logging;
using Newtonsoft.Json.Linq;


namespace AGO.Reporting.Service
{
	public class ReportingService: AbstractControllersApplication, IInitializable, IReportingService
	{
		#region Configuration and initialization

		private int processingNext;
		private readonly SequentialTimer runTaskTimer;
		private readonly Dictionary<Guid, AbstractReportWorker> runningWorkers;
		private readonly ReaderWriterLockSlim rwlock;
		private readonly SequentialTimer cleanFinishedTaskTimer;
		private TemplateResolver resolver;
		private ProjectSelector selector;

		public ReportingService()
		{
			runTaskTimer = new SequentialTimer(ProcessWaitingTasks);
			runningWorkers = new Dictionary<Guid, AbstractReportWorker>();
			rwlock = new ReaderWriterLockSlim();
			cleanFinishedTaskTimer = new SequentialTimer(CleanFinishedTasks);
			TaskScheduler.UnobservedTaskException += (sender, args) =>
			{
				Log.Error("Unobserver exception in reporting service task", args.Exception);
				args.SetObserved();
			};
		}

		private ILog log;
		private ILog Log
		{
			get { return log ?? (log = LogManager.GetLogger(GetType())); }
		}

		public override IKeyValueProvider KeyValueProvider
		{
			get
			{
				_KeyValueProvider = _KeyValueProvider ?? new AppSettingsKeyValueProvider();
				return _KeyValueProvider;
			}
			set { base.KeyValueProvider = value; }
		}

		protected override void DoRegisterCoreServices()
		{
			base.DoRegisterCoreServices();

			IocContainer.Register<IReportingRepository, ReportingRepository>();

			ReadConfiguration();
			ApplyConfiguration();
		}

		protected override void DoInitializeSingletons()
		{
			base.DoInitializeSingletons();
			resolver = new TemplateResolver(TemplatesCacheDirectory);
			selector = new ProjectSelector(ServicedProjects);
		}

		private void ReadConfiguration()
		{
			ServicedProjects = KeyValueProvider.Value("Reporting_ServicedProjects") ?? "*";
			TemplatesCacheDirectory = KeyValueProvider.Value("Reporting_TemplatesCacheDirectory");
			int n;
			RunWorkersInterval = int.TryParse(KeyValueProvider.Value("Reporting_RunWorkersInterval"), out n) ? n : 1*60*1000; //1 раз в минуту
			TrackProgressInterval = int.TryParse(KeyValueProvider.Value("Reporting_TrackProgressInterval"), out n) ? n : 2000; //раз в 2 секунды
			ConcurrentWorkersLimit = int.TryParse(KeyValueProvider.Value("Reporting_ConcurrentWorkersLimit"), out n) ? n : 5;
			ConcurrentWorkersMemoryLimitInMb = 
				int.TryParse(KeyValueProvider.Value("Reporting_ConcurrentWorkersMemoryLimitInMb"), out n) ? n : 512;
			ConcurrentWorkersTimeout = 
				int.TryParse(KeyValueProvider.Value("Reporting_ConcurrentWorkersTimeout"), out n) ? n : 60*60*24; //сутки
			CleanFinishedWorkersInterval =
				int.TryParse(KeyValueProvider.Value("Reporting_CleanFinishedWorkersInterval"), out n) ? n : 1 * 60 * 1000; //1 раз в минуту
		}

		private void ApplyConfiguration()
		{
			runTaskTimer.Interval = RunWorkersInterval;
			cleanFinishedTaskTimer.Interval = CleanFinishedWorkersInterval;
		}

		protected override void DoInitializeCoreServices()
		{
			base.DoInitializeCoreServices();

			NotificationService.SubscribeToRunReport(RunReport);
			NotificationService.SubscribeToCancelReport(id => CancelReport(id));
		}

		protected override void DoInitializeApplication()
		{
			base.DoInitializeApplication();
			
			cleanFinishedTaskTimer.Run();
			//run with 0, so, immediate query work queue and start work, if any
			runTaskTimer.Run(0);
		}

		/// <summary>
		/// Проекты, задачи которых обслуживаются сервисом.
		/// Можно задать код проекта, несколько кодов проектов через запятую или * (все проекты)
		/// </summary>
		public string ServicedProjects { get; set; }

		/// <summary>
		/// Путь к папке, в которой распологается кеш шаблонов отчетов
		/// </summary>
		private string TemplatesCacheDirectory { get; set; }

		/// <summary>
		/// Интервал запуска новых отчетов из очереди задач
		/// </summary>
		private int RunWorkersInterval { get; set; }

		/// <summary>
		/// Интервал сохранения прогресса выполнения
		/// </summary>
		private int TrackProgressInterval { get; set; }

		/// <summary>
		/// Количество одновременно запущенных worker-ов
		/// </summary>
		private int ConcurrentWorkersLimit { get; set; }

		/// <summary>
		/// Количество памяти, при превышении которого новые worker-ы не запускаются
		/// </summary>
		private int ConcurrentWorkersMemoryLimitInMb { get; set; }

		/// <summary>
		/// Таймаут работы одного worker-а
		/// </summary>
		private int ConcurrentWorkersTimeout { get; set; }

		/// <summary>
		/// Интервал очистки списка запущенных worker-ов от завершившихся нештатным образом 
		/// (штатно воркер удаляется из списка когда заканчивает работу и дергает событие)
		/// </summary>
		private int CleanFinishedWorkersInterval { get; set; }

		#endregion

		#region IReportingService implementation

		public void Dispose()
		{
			runTaskTimer.Stop();
			cleanFinishedTaskTimer.Stop();

			StopAllWorkers();

			NotificationService.Dispose();

			rwlock.Dispose();//buggable place. if some of datagenerators don't stop on cancel call and after service disposing will be stopped, 
							 //there may be exception, when RemoveWorker called on disposed rwlock. But this situation - shutdown of service -
							 //not sou dangerous
		}

		public bool Ping()
		{
			return true;
		}

		public void RunReport(Guid taskId)
		{
			runTaskTimer.Run(0);
		}

		public bool CancelReport(Guid taskId)
		{
			var canceled = StopWorker(taskId);
			runTaskTimer.Run(100); //slight time later, give worker a chance to finish and remove from running list
			return canceled;
		}

		public bool IsRunning(Guid taskId)
		{
			return HasWorker(taskId, rw => !rw.Finished);
		}

		#endregion

		private void ProcessWaitingTasks()
		{
			if (0 != Interlocked.CompareExchange(ref processingNext, 0, 1)) return;

			try
			{
				Func<bool> maxWorkersExceed = () => CalculateRunningWorkers() >= ConcurrentWorkersLimit;
				Func<bool> maxMemoryExceed = () => CalculateMemoryBarier() >= ConcurrentWorkersMemoryLimitInMb;
				var repository = IocContainer.GetInstance<IReportingRepository>();
				while (!maxWorkersExceed() && !maxMemoryExceed())
				{
					var qi = WorkQueue.Get(selector.NextProject(WorkQueue.UniqueProjects));
					if (qi == null)
					{
						//work queue is empty
						break;
					}
					DALHelper.Do(SessionProviderRegistry, qi.Project, (mainSess, projSess) =>
					{
						var task = repository.GetTask(projSess, qi.TaskId);
						if (task == null)
						{
							//странно, задачу запустили и потом сразу удалили? такое может случиться
							Log.WarnFormat("ReportTask with id '{0}' not found and can't be runned. Ignore.", qi.TaskId);
							return;
						}
						if (task.State != ReportTaskState.NotStarted || HasWorker(task.Id, w => true))
						{
							//уже запущена, либо отменена до запуска, но вообще непонятно как такое могло произойти. 
							//накладка по вызовам таймера?
							Log.WarnFormat(
								"ReportTask with id '{0}' has invalid state for running ({1}) or worker already runned for this task. Ignore.",
								task.Id, task.State);
							return;
						}
						try
						{

							var worker = CreateWorker(task);
							AddWorker(worker);
							worker.Start();
							worker.End += RemoveWorker;
						}
						catch (Exception ex)
						{
							task.State = ReportTaskState.Error;
							task.ErrorMsg = ex.Message;
							task.ErrorDetails = ex.ToString();
							projSess.SaveOrUpdate(task);
							projSess.Flush();
						}
					});
				}
			}
			catch (ThreadAbortException)
			{
				throw;
			}
			catch (OutOfMemoryException)
			{
				throw;
			}
			catch (Exception ex)
			{
				Log.Error("Ошибка при обработке задач к запуску", ex);
			}
			finally
			{
				Interlocked.Exchange(ref processingNext, 0);
				runTaskTimer.Run();
			}
		}

		private static long CalculateMemoryBarier()
		{
			return GC.GetTotalMemory(true) / 1024 / 1024;
		}

		private AbstractReportWorker CreateWorker(IReportTask task)
		{
			var worker = task.Setting.GeneratorType == GeneratorType.CustomGenerator
			             	? (AbstractReportWorker) new CustomReportWorker(task.ProjectCode, task.Id, IocContainer, resolver)
							: new ReportWorker(task.ProjectCode, task.Id, IocContainer, resolver);
			if (!task.Parameters.IsNullOrWhiteSpace())
			{
				var tokenReader = new JTokenReader(JToken.Parse(task.Parameters)) { CloseInput = false };
				var paramType = Type.GetType(task.Setting.ReportParameterType, true);
				worker.Parameters = JsonService.CreateSerializer().Deserialize(tokenReader, paramType);
			}
			worker.UserCulture = CultureInfo.GetCultureInfo(task.Culture);
			worker.Timeout = ConcurrentWorkersTimeout * 1000;
			worker.TrackProgressInterval = TrackProgressInterval;
			
			worker.Prepare(task);

			return worker;
		}

		private void AddWorker(AbstractReportWorker worker)
		{
			rwlock.EnterUpgradeableReadLock();
			try
			{
				if (runningWorkers.ContainsKey(worker.TaskId)) return;
				rwlock.EnterWriteLock();
				try
				{
					if (runningWorkers.ContainsKey(worker.TaskId)) return;
					runningWorkers.Add(worker.TaskId, worker);
				}
				finally
				{
					rwlock.ExitWriteLock();
				}
			}
			finally
			{
				rwlock.ExitUpgradeableReadLock();
			}
		}

		private int CalculateRunningWorkers()
		{
			rwlock.EnterReadLock();
			try
			{
				return runningWorkers.Count;
			}
			finally
			{
				rwlock.ExitReadLock();
			}
		}

		private bool HasWorker(Guid taskId, Func<AbstractReportWorker, bool> predicate)
		{
			rwlock.EnterReadLock();
			try
			{
				return runningWorkers.ContainsKey(taskId) && predicate(runningWorkers[taskId]);
			}
			finally
			{
				rwlock.ExitReadLock();
			}
		}

		private void RemoveWorker(object sender, EventArgs e)
		{
			var worker = (AbstractReportWorker) sender;
			worker.End -= RemoveWorker;
			RemoveWorker(worker.TaskId);
			runTaskTimer.Run(0);
		}

		private AbstractReportWorker RemoveWorker(Guid taskId)
		{
			rwlock.EnterUpgradeableReadLock();
			try
			{
				if (!runningWorkers.ContainsKey(taskId)) return null;
				rwlock.EnterWriteLock();
				try
				{
					if (!runningWorkers.ContainsKey(taskId)) return null;
					var worker = runningWorkers[taskId];
					runningWorkers.Remove(taskId);
					return worker;
				}
				finally
				{
					rwlock.ExitWriteLock();
				}
			}
			finally
			{
				rwlock.ExitUpgradeableReadLock();
			}
		}

		private bool StopWorker(Guid taskId)
		{
			var worker = RemoveWorker(taskId);
			if (worker != null && !worker.Finished)
			{
				worker.Stop();
				return true;
			}
			return false;
		}

		private void StopAllWorkers()
		{
			rwlock.EnterWriteLock();
			try
			{
				foreach (var worker in runningWorkers.Values)
				{
					worker.Stop();
				}
				runningWorkers.Clear();
			}
			finally
			{
				rwlock.ExitWriteLock();
			}
		}

		private void CleanFinishedTasks()
		{
			if (runningWorkers.Count > 0)
			{
				rwlock.EnterUpgradeableReadLock();
				try
				{
					if (runningWorkers.Count > 0)
					{
						//Избавился от проверок с помощью Linq, т.к. в закешированных выражения зависают ReportWorker-ы,
						//чем порождают перерасход памяти
						var finishedWorkerIds = runningWorkers.Keys.Where(taskId => runningWorkers[taskId].Finished).ToList();
						if (finishedWorkerIds.Count > 0)
						{
							rwlock.EnterWriteLock();
							try
							{
								foreach (var taskId in finishedWorkerIds)
									runningWorkers.Remove(taskId);
							}
							finally
							{
								rwlock.ExitWriteLock();
							}
						}
					}
				}
				finally
				{
					rwlock.ExitUpgradeableReadLock();
				}
			}
			cleanFinishedTaskTimer.Run();
		}
	}
}
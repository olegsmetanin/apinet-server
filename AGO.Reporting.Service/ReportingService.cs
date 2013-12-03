using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using AGO.Core;
using AGO.Core.Application;
using AGO.Core.Config;
using AGO.Core.Execution;
using AGO.Core.Model.Reporting;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;
using AGO.Reporting.Service;
using Common.Logging;
using Newtonsoft.Json;
using SimpleInjector.Integration.Web.Mvc;
using WebActivator;

[assembly: PostApplicationStartMethod(typeof(Initializer), "Initialize")]

namespace AGO.Reporting.Service
{
	public class ReportingService: AbstractPersistenceApplication, IInitializable, IReportingService
	{
		#region Configuration and initialization

		private readonly List<Guid> waitingForRun;
		private readonly Timer runTaskTimer;
		private readonly Dictionary<Guid, AbstractReportWorker> runningWorkers;
		private readonly ReaderWriterLockSlim rwlock;
		private readonly Timer cleanFinishedTaskTimer;
		private TemplateResolver resolver;

		public ReportingService()
		{
			waitingForRun = new List<Guid>();
			runTaskTimer = new Timer(ProcessWaitingTasks, null, Timeout.Infinite, Timeout.Infinite);
			runningWorkers = new Dictionary<Guid, AbstractReportWorker>();
			rwlock = new ReaderWriterLockSlim();
			cleanFinishedTaskTimer = new Timer(CleanFinishedTasks, null, Timeout.Infinite, Timeout.Infinite);
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
				_KeyValueProvider = _KeyValueProvider ?? new AppSettingsKeyValueProvider(
					WebConfigurationManager.OpenWebConfiguration("~/Web.config"));
				return _KeyValueProvider;
			}
			set { base.KeyValueProvider = value; }
		}

		protected override void DoRegisterCoreServices()
		{
			base.DoRegisterCoreServices();

			IocContainer.Register<IReportingRepository, ReportingRepository>();
			IocContainer.RegisterSingle<IActionExecutor, ActionExecutor>();

			ReadConfiguration();
		}

		protected override void DoInitializeSingletons()
		{
			base.DoInitializeSingletons();
			resolver = new TemplateResolver(IocContainer.GetInstance<IReportingRepository>(), TemplatesCacheDirectory);
		}

		private void ReadConfiguration()
		{
			ServiceName = KeyValueProvider.Value("Reporting_ServiceName");
			TemplatesCacheDirectory = KeyValueProvider.Value("Reporting_TemplatesCacheDirectory");
			int n;
			RunWorkersInterval = int.TryParse(KeyValueProvider.Value("Reporting_RunWorkersInterval"), out n) ? n : 100; //10 раз в секунду
			ConcurrentWorkersLimit = int.TryParse(KeyValueProvider.Value("Reporting_ConcurrentWorkersLimit"), out n) ? n : 5;
			ConcurrentWorkersMemoryLimitInMb = 
				int.TryParse(KeyValueProvider.Value("Reporting_ConcurrentWorkersMemoryLimitInMb"), out n) ? n : 512;
			ConcurrentWorkersTimeout = 
				int.TryParse(KeyValueProvider.Value("Reporting_ConcurrentWorkersTimeout"), out n) ? n : 60*60*24; //сутки
			CleanFinishedWorkersInterval = 
				int.TryParse(KeyValueProvider.Value("Reporting_CleanFinishedWorkersInterval"), out n) ? n : 5000; //5 секунд
		}

		protected override void DoInitializeCoreServices()
		{
			base.DoInitializeCoreServices();
//			if (!WebEnabled)
//				return;

			RegisterReportingRoutes(RouteTable.Routes);
			DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(IocContainer));
		}

		protected void RegisterReportingRoutes(RouteCollection routes)
		{
			routes.RouteExistingFiles = false;

			routes.MapRoute("api", "api/{method}", new { controller = "ReportingApi", action="Dispatch", service = this });
		
			//TODO default route for error
		}

		/// <summary>
		/// Имя текущего сервиса отчетов
		/// </summary>
		private string ServiceName { get; set; }

		/// <summary>
		/// Путь к папке, в которой распологается кеш шаблонов отчетов
		/// </summary>
		private string TemplatesCacheDirectory { get; set; }

		/// <summary>
		/// Интервал запуска новых отчетов из очереди ожидания
		/// </summary>
		private int RunWorkersInterval { get; set; }

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
		/// Интервал очистки списка запущенных worker-ов от завершившихся
		/// </summary>
		private int CleanFinishedWorkersInterval { get; set; }

		#endregion

		#region IReportingService implementation

		public void Dispose()
		{
			//TODO
		}

		public bool Ping()
		{
			return true;
		}

		public void RunReport(Guid taskId)
		{
			AddWaitingTask(taskId);
		}

		public bool CancelReport(Guid taskId)
		{
			throw new NotImplementedException();
		}

		public bool IsRunning(Guid taskId)
		{
			return HasWorker(taskId, rw => !rw.Finished);
		}

		public bool IsWaitingForRun(Guid taskId)
		{
			lock (waitingForRun)
			{
				return waitingForRun.Contains(taskId);
			}
		}

		#endregion

		private void AddWaitingTask(Guid taskId)
		{
			lock(waitingForRun)
			{
				if (!waitingForRun.Contains(taskId))
					waitingForRun.Add(taskId);
			}
			runTaskTimer.Change(0, Timeout.Infinite);
		}

		private void ProcessWaitingTasks(object state)
		{
			if (waitingForRun.Count <= 0) return;
			lock (waitingForRun)
			{
				if (waitingForRun.Count <= 0) return;

				try
				{
					var processed = new List<Guid>();
					var repository = IocContainer.GetInstance<IReportingRepository>();
					foreach (var taskId in waitingForRun)
					{
						//Запускаем не больше лимита
						if (runningWorkers.Count >= ConcurrentWorkersLimit) break;
						//Перед запуском проверяем ограничение по кол-ву используемой памяти
						if (CalculateMemoryBarier() >= ConcurrentWorkersMemoryLimitInMb) break;

						var task = repository.GetTask(taskId);
						if (task == null)
						{
							//странно, задачу запустили и потом сразу удалили? такое может случиться
							processed.Add(taskId);
							continue;
						}
						try
						{
							if (task.State != ReportTaskState.NotStarted || HasWorker(task.Id, w => true))
							{
								//уже запущена, либо отменена до запуска
								processed.Add(taskId);
								continue;
							}
							var worker = CreateWorker(task);
							AddWorker(worker);
							worker.Start();
							processed.Add(taskId);
						}
						catch (Exception ex)
						{
							task.State = ReportTaskState.Error;
							task.ErrorMsg = ex.Message;
							task.ErrorDetails = ex.ToString();
							SessionProvider.CurrentSession.SaveOrUpdate(task);
							SessionProvider.CurrentSession.Flush();
						}
					}

					//Все обработанные удаляем из очереди, неважно нормально они запущени или были
					//какие-то проблемы. Они по идее записаны в таск и/или лог, и в повторной обработке
					//эти задачи не нуждаются
					foreach (var taskId in processed)
					{
						waitingForRun.Remove(taskId);
					}
					//Планируем свой следующий запуск, если необходимо
					if (waitingForRun.Count > 0)
					{
						runTaskTimer.Change(RunWorkersInterval, Timeout.Infinite);
					}
				}
				catch(ThreadAbortException)
				{
					throw;
				}
				catch(OutOfMemoryException)
				{
					throw;
				}
				catch (Exception ex)
				{
					Log.Error("Ошибка при обработке задач к запуску", ex);
				}
			}
		}

		private static long CalculateMemoryBarier()
		{
			return GC.GetTotalMemory(true) / 1024 / 1024;
		}

		private AbstractReportWorker CreateWorker(IReportTask task)
		{
			var worker = task.Setting.GeneratorType == GeneratorType.CustomGenerator
			             	? (AbstractReportWorker) new CustomReportWorker()
			             	: new ReportWorker();
			worker.TaskId = task.Id;
			worker.TemplateResolver = resolver;
			worker.Parameters = !task.Parameters.IsNullOrWhiteSpace()
			                    	? JsonConvert.DeserializeObject(task.Parameters, Type.GetType(task.Setting.ReportParameterType, true))
			                    	: null;
			worker.Timeout = new TimeSpan(ConcurrentWorkersTimeout * 1000);
			//TODO session??

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


		private bool HasWorker(Guid taskId, Func<AbstractReportWorker, bool> predicate)
		{
			rwlock.EnterUpgradeableReadLock();
			try
			{
				return runningWorkers.ContainsKey(taskId) && predicate(runningWorkers[taskId]);
			}
			finally
			{
				rwlock.ExitUpgradeableReadLock();
			}
		}

		private void CleanFinishedTasks(object state)
		{
			if (runningWorkers.Count <= 0) return;

			rwlock.EnterUpgradeableReadLock();
			try
			{
				if (runningWorkers.Count <= 0) return;
				//Избавился от проверок с помощью Linq, т.к. в закешированных выражения зависают ReportWorker-ы,
				//чем порождают перерасход памяти
				var finishedWorkerIds = runningWorkers.Keys.Where(taskId => runningWorkers[taskId].Finished).ToList();
				if (finishedWorkerIds.Count <= 0) return;
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
			finally
			{
				rwlock.ExitUpgradeableReadLock();
			}
		}
	}

	public static class Initializer
	{
		public static void Initialize()
		{
			new ReportingService().Initialize();
		}
	}
}
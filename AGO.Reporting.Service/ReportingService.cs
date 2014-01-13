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
using AGO.Notifications;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;
using AGO.Reporting.Service.Controllers;
using Common.Logging;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json.Linq;
using SimpleInjector.Integration.Web.Mvc;


namespace AGO.Reporting.Service
{
	public class ReportingService: AbstractPersistenceApplication, IInitializable, IReportingService
	{
		#region Configuration and initialization

		private readonly List<Guid> waitingForRun;
		private readonly SequentialTimer runTaskTimer;
		private readonly Dictionary<Guid, AbstractReportWorker> runningWorkers;
		private readonly ReaderWriterLockSlim rwlock;
		private readonly SequentialTimer cleanFinishedTaskTimer;
		private TemplateResolver resolver;

		public ReportingService()
		{
			waitingForRun = new List<Guid>();
			runTaskTimer = new SequentialTimer(ProcessWaitingTasks);
			runningWorkers = new Dictionary<Guid, AbstractReportWorker>();
			rwlock = new ReaderWriterLockSlim();
			cleanFinishedTaskTimer = new SequentialTimer(CleanFinishedTasks);
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
			IocContainer.RegisterAll<IActionParameterResolver>(new [] { typeof(JsonBodyParameterResolver) });
			IocContainer.RegisterAll<IActionParameterTransformer>(
				new [] { typeof(AttributeValidatingParameterTransformer), typeof(JsonTokenParameterTransformer) });
			IocContainer.RegisterSingle<IActionExecutor, ActionExecutor>();

			IocContainer.RegisterSingle<IHubContext>(() => GlobalHost.ConnectionManager.GetHubContext<NotificationsHub>());

			ReadConfiguration();
			ApplyConfiguration();
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
			TrackProgressInterval = int.TryParse(KeyValueProvider.Value("Reporting_TrackProgressInterval"), out n) ? n : 2000; //раз в 2 секунды
			ConcurrentWorkersLimit = int.TryParse(KeyValueProvider.Value("Reporting_ConcurrentWorkersLimit"), out n) ? n : 5;
			ConcurrentWorkersMemoryLimitInMb = 
				int.TryParse(KeyValueProvider.Value("Reporting_ConcurrentWorkersMemoryLimitInMb"), out n) ? n : 512;
			ConcurrentWorkersTimeout = 
				int.TryParse(KeyValueProvider.Value("Reporting_ConcurrentWorkersTimeout"), out n) ? n : 60*60*24; //сутки
			CleanFinishedWorkersInterval = 
				int.TryParse(KeyValueProvider.Value("Reporting_CleanFinishedWorkersInterval"), out n) ? n : 5000; //5 секунд
		}

		private void ApplyConfiguration()
		{
			runTaskTimer.Interval = RunWorkersInterval;
			cleanFinishedTaskTimer.Interval = CleanFinishedWorkersInterval;
		}

		protected override void DoInitializeCoreServices()
		{
			base.DoInitializeCoreServices();

			RegisterReportingRoutes(RouteTable.Routes);
			DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(IocContainer));
		}

		protected void RegisterReportingRoutes(RouteCollection routes)
		{
			//Have't any web resources
			routes.RouteExistingFiles = false;
			//Our api
			routes.MapRoute("api", "api/{method}", new { controller = "ReportingApi", action="Dispatch", service = this });
			//default route (error)
			routes.MapRoute("any", "{*value}", new {controller = "ReportingApi", action = "Error"});
		}

		protected override void DoInitializeApplication()
		{
			base.DoInitializeApplication();
			cleanFinishedTaskTimer.Run();
		}

		/// <summary>
		/// Имя текущего сервиса отчетов
		/// </summary>
		//TODO use in logging or remove (and from reporttask too)
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
		/// Интервал очистки списка запущенных worker-ов от завершившихся
		/// </summary>
		private int CleanFinishedWorkersInterval { get; set; }

		#endregion

		#region IReportingService implementation

		public void Dispose()
		{
			runTaskTimer.Stop();
			cleanFinishedTaskTimer.Stop();

			rwlock.Dispose();

			RouteTable.Routes.Clear();
			//TODO other shutdown (timers etc)
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
			bool canceled;
			lock (waitingForRun)
			{
				canceled = waitingForRun.Remove(taskId);
			}
			if (!canceled)
			{
				canceled = StopWorker(taskId);
			}
			return canceled;
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
			runTaskTimer.Run(0);
		}

		private void ProcessWaitingTasks()
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
							SessionProvider.FlushCurrentSession();
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
						runTaskTimer.Run();
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
			             	? (AbstractReportWorker) new CustomReportWorker(task.Id, IocContainer, resolver)
							: new ReportWorker(task.Id, IocContainer, resolver);
			if (!task.Parameters.IsNullOrWhiteSpace())
			{
				var tokenReader = new JTokenReader(JToken.Parse(task.Parameters)) { CloseInput = false };
				var paramType = Type.GetType(task.Setting.ReportParameterType, true);
				worker.Parameters = JsonService.CreateSerializer().Deserialize(tokenReader, paramType);
			}
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
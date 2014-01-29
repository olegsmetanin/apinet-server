using AGO.Core.Watchers;

namespace AGO.Core.Application
{
	/// <summary>
	/// Приложение-хост для разного рода наблюдателей и оповещателей.
	/// Пока только один наблюдатель - следит за очередью задач и рассылает уведомления об изменении номера в очереди.
	/// </summary>
	public class WatcherApplication: AbstractPersistenceApplication
	{
		public WorkQueueWatchService QueueWatcher { get; private set; }

		protected override void DoRegisterCoreServices()
		{
			base.DoRegisterCoreServices();

			DoRegisterWatchers();
		}

		protected virtual void DoRegisterWatchers()
		{
			IocContainer.RegisterSingle<WorkQueueWatchService>();
		}

		protected override void DoInitializeCoreServices()
		{
			base.DoInitializeCoreServices();

			DoInitializeWatchers();
		}

		protected virtual void DoInitializeWatchers()
		{
			QueueWatcher = IocContainer.GetInstance<WorkQueueWatchService>();
		}
	}
}
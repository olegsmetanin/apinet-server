using System;
using System.Threading.Tasks;

namespace AGO.Core.Notification
{
	public class NoopNotificationService: AbstractService, INotificationService
	{
		#region Service implementation

		public Task EmitRunReport(Guid reportId)
		{
			return null;
		}

		public Task EmitCancelReport(Guid reportId)
		{
			return null;
		}

		public Task EmitReportChanged(string type, string login, object dto)
		{
			return null;
		}

		public void SubscribeToRunReport(Action<Guid> subscriber)
		{
		}

		public void SubscribeToCancelReport(Action<Guid> subscriber)
		{
		}

		public void SubscribeToReportChanged(Action<string, string, object> subscriber)
		{
		}

		public Task EmitWorkQueueChanged(string login, object dto)
		{
			return null;
		}
		public void Dispose()
		{
		}

		#endregion
	}
}

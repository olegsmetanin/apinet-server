using System;
using System.Threading.Tasks;

namespace AGO.Core.Notification
{
	/// <summary>
	/// Сервис оповещений для межсервисного общения внутри приложения и для comet-like 
	/// оповещений клиентов
	/// </summary>
	public interface INotificationService: IDisposable
	{
		Task EmitRunReport(Guid reportId);

		Task EmitCancelReport(Guid reportId);

		Task EmitReportChanged(object dto);

		Task EmitReportDeleted(object dto);

		void SubscribeToRunReport(Action<Guid> subscriber);

		void SubscribeToCancelReport(Action<Guid> subscriber);
	}
}

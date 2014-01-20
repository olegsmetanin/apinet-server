using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGO.Core.Notification
{
	/// <summary>
	/// Сервис оповещений для межсервисного общения внутри приложения и для comet-like 
	/// оповещений клиентов
	/// </summary>
	public interface INotificationService
	{
		Task EmitRunReport(Guid reportId);

		Task EmitCancelReport(Guid reportId);

		Task EmitReportChanged(object dto);

		void SubscribeToRunReport(Action<Guid> subscriber);

		void SubscribeToCancelReport(Action<Guid> subscriber);
	}
}

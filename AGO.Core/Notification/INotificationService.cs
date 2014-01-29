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
		/// <summary>
		/// Оповещение о необходимости запустить отчет на выполнение
		/// </summary>
		/// <param name="reportId">Идентификатор задачи отчета</param>
		/// <remarks>Для межсервисного взаимодействия</remarks>
		Task EmitRunReport(Guid reportId);

		/// <summary>
		/// Оповещение о необходимости прервать выполняющийся отчет
		/// </summary>
		/// <param name="reportId">Идентификатор задачи отчета</param>
		/// <remarks>Для межсервисного взаимодействия</remarks>
		Task EmitCancelReport(Guid reportId);

		/// <summary>
		/// Оповещение об изменении состояния задачи (статус, % выполнения либо удаление задачи)
		/// </summary>
		/// <param name="type">Тип изменения</param>
		/// <param name="login">Логин пользователя, которому адресовано оповещение</param>
		/// <param name="dto">Данные задачи отчета</param>
		/// <remarks>Для оповещения оконечных клиентских приложений</remarks>
		Task EmitReportChanged(string type, string login, object dto);

		void SubscribeToRunReport(Action<Guid> subscriber);

		void SubscribeToCancelReport(Action<Guid> subscriber);

		void SubscribeToReportChanged(Action<string, string, object> subscriber);

		Task EmitWorkQueueChanged(string login, object dto);
	}
}

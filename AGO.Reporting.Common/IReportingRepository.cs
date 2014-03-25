using System;
using AGO.Reporting.Common.Model;
using NHibernate;

namespace AGO.Reporting.Common
{
	/// <summary>
	/// Интерфейс репозитория для доступа сервиса отчетов к данным о настройке 
	/// и другой необходимой для работы информации (кроме данных самих отчетов, это другой механизм)
	/// </summary>
	/// <remarks>
	/// Введен с целью отделить сущности для работы сервис отчетов от их реализации в Core и избежать циклических ссылок.
	/// Также должен помочь в будущем отделить сервис отчетов в отдельный проект и сделать его подключение нугет пакетом каким-нибудь.
	/// Сессия в сигнатурах методов вследствие того, что на этом уровне мы не можем контролировать время жизни сессии, а отчет и шаблон
	/// оба возвращаются в виде прокси (т.к. содержат бинарные данные и имеют lazy поля поэтому)
	/// </remarks>
	public interface IReportingRepository
	{
		IReportTask GetTask(ISession session, Guid taskId);

		object GetTaskAsDTO(ISession mainDbSession, ISession projectDbSession, Guid taskId);

		IReportTemplate GetTemplate(ISession session, Guid templateId);

		void ArchiveReport(ISession mainDbSession, IReportTask report);
	}
}
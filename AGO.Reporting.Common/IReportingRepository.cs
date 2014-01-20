using System;
using System.Collections.Generic;
using AGO.Reporting.Common.Model;

namespace AGO.Reporting.Common
{
	/// <summary>
	/// Интерфейс репозитория для доступа сервиса отчетов к данным о настройке 
	/// и другой необходимой для работы информации (кроме данных самих отчетов, это другой механизм)
	/// </summary>
	public interface IReportingRepository
	{
		IEnumerable<IReportingServiceDescriptor> GetAllDescriptors();

		IReportingServiceDescriptor GetDescriptor(string name);

		IReportTask GetTask(Guid taskId);

		object GetTaskAsDTO(Guid taskId);

		IReportTemplate GetTemplate(Guid templateId);
	}
}
using System;


namespace AGO.Reporting.Common
{

    /// <summary>
    /// Интерфейс сервиса генерации отчетов
    /// </summary>
    public interface IReportingService: IDisposable
    {
        /// <summary>
        /// Метод для проверки наличия связи с сервисом
        /// </summary>
        /// <returns>Всегда возвращает <b>true</b></returns>
        bool Ping();

        /// <summary>
        /// Планирует выполнение задачи по созданию отчета в сервисе отчетов
        /// </summary>
        /// <param name="taskId">Идентификатор задач</param>
        void RunReport(Guid taskId);

        /// <summary>
        /// Отменяет выполняющуюся задачу по созданию отчета
        /// </summary>
        /// <param name="taskId">Идентификатор задачи</param>
        /// <returns><b>true</b>, если задача выполнялась и сервис успешно отменил ее выполнение</returns>
        bool CancelReport(Guid taskId);

        /// <summary>
        /// Узнает у сервиса, выполняется ли задача в настоящий момент
        /// </summary>
        /// <param name="taskId">Идентификатор задачи</param>
        /// <returns><b>true</b>, если задача в настоящий момент выполняется</returns>
        bool IsRunning(Guid taskId);

        /// <summary>
        /// Узнает у сервиса, ожидает ли задача выполнения в настоящий момент
        /// </summary>
        /// <param name="taskid">Идентификатор задачи</param>
        /// <returns><b>true</b>, если задача в настоящий момент ожидает выполнения</returns>
        bool IsWaitingForRun(Guid taskid);
    }
}
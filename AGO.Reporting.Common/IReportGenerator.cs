using System;
using System.IO;
using System.Xml;


namespace AGO.Reporting.Common
{
    /// <summary>
    /// Результат работы генератора отчетов
    /// </summary>
    public interface IReportGeneratorResult
    {
        /// <summary>
        /// Возвращает результат работы генератора в виде потока
        /// <exception cref="NotSupportedException">Возникает, если результат работы не может быть сохранен в поток</exception>
        /// </summary>
        Stream Result { get; }

        /// <summary>
        /// Возвращает предлогаемое по умолчанию имя для файла отчета
        /// </summary>
        string FileName { get; set; }
    }

    /// <summary>
    /// Этот интерфейс должны поддерживать генераторы отчетов в разных форматах.
    /// </summary>
    public interface IReportGenerator: IReportGeneratorResult
    {
        /// <summary>
        /// Запускает процесс создания отчета (либо иной формы документа)
        /// </summary>
        /// <param name="pathToTemplate">Путь к шаблону отчета. Если для генерации документа он
        /// не нужен, то может быть null или пустой строкой, зависит от потребностей самого генератора</param>
        /// <param name="data">Данные, по которым необходимо построить отчет</param>
        void MakeReport(string pathToTemplate, XmlDocument data);
    }

    /// <summary>
    /// Интерфейс генераторов отчетов, реализующих "особую" логику создания отчета, не 
    /// соответствующую типовому процессу подготовки данных в формате xml и переноса их
    /// в шаблон отчета.
    /// </summary>
    public interface ICustomReportGenerator: IReportGeneratorResult
    {
        /// <summary>
        /// Генерирует отчет
        /// </summary>
        /// <param name="parameters">Параметры для генерации отчета (json)</param>
        /// <param name="templateResolver">Делегат для разрешения идентификатора шаблона отчета в путь к файлу шаблона</param>
        /// <param name="mainTemplateId">Идннтификатор основного шаблона отчета</param>
        void MakeReport(string parameters, Func<Guid, string> templateResolver, Guid mainTemplateId);
    }
}
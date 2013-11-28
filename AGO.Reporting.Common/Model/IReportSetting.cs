namespace AGO.Reporting.Common.Model
{
	/// <summary>
	/// Настройка отчета
	/// </summary>
	public interface IReportSetting: IReportingModel
	{
		/// <summary>
		/// Тип генератора отчета
		/// </summary>
		GeneratorType GeneratorType { get; }

		/// <summary>
		/// Имя класса генератора данных для отчета (assembly qualified)
		/// </summary>
		string DataGeneratorType { get; }

		/// <summary>
		/// Имя класса параметров для отчета (assembly qualified)
		/// </summary>
		string ReportParameterType { get; }

		/// <summary>
		/// Шаблон отчета
		/// </summary>
		IReportTemplate Template { get; }
	}
}
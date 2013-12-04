using System;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;
using AGO.Reporting.Service.ReportGenerators;

namespace AGO.Reporting.Service
{
	public class ReportWorker: AbstractReportWorker
	{
		private string pathToTemplate;
		private IReportDataGenerator dataGenerator;
		private IReportGenerator reportGenerator;

		public override void Prepare(IReportTask task)
		{
			pathToTemplate = TemplateResolver.Resolve(task.Setting.Template);
			dataGenerator = CreateDataGeneratorInstance(task.Setting.DataGeneratorType);
			reportGenerator = CreateReportGeneratorInstance(task.Setting.GeneratorType);
		}

		protected override IReportGeneratorResult InternalStart()
		{
			//may be start has been postponed for a long time
			TokenSource.Token.ThrowIfCancellationRequested();

			var data = dataGenerator.GetReportData(Parameters, TokenSource.Token);
			//генератор не нужен, т.к. результат уже есть у нас. можно попробовать отпустить память
			dataGenerator = null;

			//TODO закрываем сессию, т.к. она нам больше не нужна
			//???

			TokenSource.Token.ThrowIfCancellationRequested();

			reportGenerator.MakeReport(pathToTemplate, data, TokenSource.Token);
			return reportGenerator;
		}

		#region Создание параметров и генераторов

		private static IReportDataGenerator CreateDataGeneratorInstance(string typeName)
		{
			var dataGeneratorType = Type.GetType(typeName, true);
			var dataGenerator = Activator.CreateInstance(dataGeneratorType) as IReportDataGenerator;
			if (dataGenerator == null)
			{
				throw new ReportingException(string.Format("Не удалось создать экземпляр генератора данных, заданного типом '{0}'", typeName));
			}
			return dataGenerator;
		}

		private static IReportGenerator CreateReportGeneratorInstance(GeneratorType type)
		{
			switch (type)
			{
				case GeneratorType.XlsSyncFusionGenerator:
					return new SyncFusionXlsReportGenerator();
				case GeneratorType.CvsGenerator:
					return new CsvReportGenerator();
				case GeneratorType.ShapeGenerator:
					return new ShapeReportGenerator();
				case GeneratorType.RtfGenerator:
					return new RtfReportGenerator();
				case GeneratorType.DocSyncFusionGenerator:
					return new SyncFusionDocReportGenerator();
				default:
					//Тип CustomGenerator здесь специально не обрабатываем, т.к. для него отдельный
					//наследник worker-а предусмотрен.
					throw new ReportingException(string.Format("Неизвестный тип генератора отчетов: {0}", type));
			}
		}

		#endregion
	}
}
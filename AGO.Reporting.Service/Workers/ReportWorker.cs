using System;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;
using AGO.Reporting.Service.ReportGenerators;
using SimpleInjector;

namespace AGO.Reporting.Service.Workers
{
	public class ReportWorker: AbstractReportWorker
	{
		private string pathToTemplate;
		private IReportDataGenerator dataGenerator;
		private IReportGenerator reportGenerator;

		public ReportWorker(string project, Guid taskId, Container di, TemplateResolver resolver) : base(project, taskId, di, resolver)
		{
		}

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

			TokenSource.Token.ThrowIfCancellationRequested();

			reportGenerator.MakeReport(pathToTemplate, data, TokenSource.Token);
			return reportGenerator;
		}

		protected override void InternalTrackProgress(IReportTask task)
		{
			var tracker = dataGenerator as IProgressTracker;
			if (tracker != null && task.DataGenerationProgress < tracker.PercentCompleted) 
				task.DataGenerationProgress = tracker.PercentCompleted;
			tracker = reportGenerator as IProgressTracker;
			if (tracker != null && task.ReportGenerationProgress < tracker.PercentCompleted)
				task.ReportGenerationProgress = tracker.PercentCompleted;
		}

		#region Создание параметров и генераторов

		private IReportDataGenerator CreateDataGeneratorInstance(string typeName)
		{
			var dgtype = Type.GetType(typeName, true);
			var dg = Container.GetInstance(dgtype) as IReportDataGenerator;
			if (dg == null)
			{
				throw new ReportingException(string.Format("Не удалось создать экземпляр генератора данных, заданного типом '{0}'", typeName));
			}
			return dg;
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
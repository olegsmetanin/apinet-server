namespace AGO.Reporting.Common
{
	/// <summary>
	/// Тип генератора отчета - определяет выходной формат отчета,
	/// вид шаблона и принципы формирования
	/// </summary>
	public enum GeneratorType
	{
		CvsGenerator = 0,

		ShapeGenerator = 1,

		RtfGenerator = 2,

		XlsSyncFusionGenerator = 3,

		DocSyncFusionGenerator = 4,

		CustomGenerator = 5
	}
}
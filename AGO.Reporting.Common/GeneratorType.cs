namespace AGO.Reporting.Common
{
	/// <summary>
	/// Тип генератора отчета - определяет выходной формат отчета,
	/// вид шаблона и принципы формирования
	/// </summary>
	public enum GeneratorType
	{
		XlsGemboxGenerator = 0,

		CvsGenerator = 1,

		ShapeGenerator = 2,

		RtfGenerator = 3,

		XlsSyncFusionGenerator = 4,

		DocSyncFusionGenerator = 5,

		CustomGenerator = 7
	}
}
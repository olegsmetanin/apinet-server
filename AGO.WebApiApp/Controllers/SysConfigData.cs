namespace AGO.WebApiApp.Controllers
{
	public class SysConfigData
	{
		public SysConfigData(string src, string module): this(src, string.Empty, string.Empty, module)
		{
		}

		public SysConfigData(string src, string site, string project, string module)
		{
			SrcPath = src;
			SitePath = site;
			Project = project;
			Module = module;
		}

		public string SrcPath { get; set; }

		public string SitePath { get; set; }

		public string Project { get; set; }

		public string Module { get; set; }
	}
}
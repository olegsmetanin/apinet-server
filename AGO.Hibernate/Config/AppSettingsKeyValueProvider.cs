using System.Collections.Generic;
using System.Configuration;

namespace AGO.Hibernate.Config
{
	public class AppSettingsKeyValueProvider : IKeyValueProvider
	{
		private readonly Configuration _Configuration;

		public AppSettingsKeyValueProvider(Configuration configuration = null)
		{
			_Configuration = configuration ?? ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
		}

		public IEnumerable<string> Keys 
		{
			get { return _Configuration.AppSettings.Settings.AllKeys; }
		}

		public string RealKey(string key)
		{
			return key;
		}

		public string Value(string key)
		{
			var setting = _Configuration.AppSettings.Settings[key];
			return setting != null ? setting.Value : null;
		}
	}
}

using System;
using System.Collections.Generic;
using AGO.Core.Config;

namespace AGO.Reporting.Tests
{
	public class OverridableAppSettingsKeyValueProvider: IKeyValueProvider
	{
		private readonly IKeyValueProvider overrides;
		private readonly IKeyValueProvider appSettings;

		public OverridableAppSettingsKeyValueProvider(IKeyValueProvider overrides)
		{
			if (overrides == null)
				throw new ArgumentNullException("overrides");

			this.overrides = overrides;
			appSettings = new AppSettingsKeyValueProvider();
		}

		public IEnumerable<string> Keys
		{
			get { return appSettings.Keys; }
		}

		public string RealKey(string key)
		{
			return appSettings.RealKey(key);
		}

		public string Value(string key)
		{
			return overrides.Value(key) ?? appSettings.Value(key);
		}
	}
}
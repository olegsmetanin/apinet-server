using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Config;

namespace AGO.Core.Tests
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
			get { return overrides.Keys.Union(appSettings.Keys); }
		}

		public string RealKey(string key)
		{
			return overrides.RealKey(key) ?? appSettings.RealKey(key);
		}

		public string Value(string key)
		{
			return overrides.Value(key) ?? appSettings.Value(key);
		}
	}
}
using System.Collections.Generic;

namespace AGO.Core.Config
{
	public class KeyValueConfigurableDictionary: Dictionary<string, string>, IKeyValueConfigurable
	{
		#region Interfaces implementation

		public string GetConfigProperty(string key)
		{
			return ContainsKey(key) ? this[key] : null;
		}

		public void SetConfigProperty(string key, string value)
		{
			this[key] = value;
		}

		#endregion
	}
}
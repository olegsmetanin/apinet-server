using System;

namespace AGO.Core.Config
{
	public class KeyValueConfigProvider : IConfigProvider<IKeyValueConfigurable>
	{
		private readonly IKeyValueProvider _Provider;

		public void ApplyTo(IKeyValueConfigurable configurable)
		{
			foreach (var key in _Provider.Keys)
			{
				var realKey = _Provider.RealKey(key);
				if (realKey.IsNullOrWhiteSpace())
					continue;
				configurable.SetConfigProperty(realKey, _Provider.Value(key));
			}			
		}

		public KeyValueConfigProvider(IKeyValueProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");
			_Provider = provider;
		}
	}
}

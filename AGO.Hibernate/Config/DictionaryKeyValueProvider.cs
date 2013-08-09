using System;
using System.Collections.Generic;

namespace AGO.Hibernate.Config
{
	public class DictionaryKeyValueProvider : IKeyValueProvider
	{
		private readonly IDictionary<string, string> _Source;

		public DictionaryKeyValueProvider(IDictionary<string, string> source)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			_Source = source;
		}

		public IEnumerable<string> Keys 
		{
			get { return _Source.Keys; }
		}

		public string RealKey(string key)
		{
			return key;
		}

		public string Value(string key)
		{
			return _Source.ContainsKey(key) ? _Source[key] : null;
		}
	}
}

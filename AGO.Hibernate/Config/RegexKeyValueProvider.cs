using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AGO.Hibernate.Config
{
	public class RegexKeyValueProvider : IKeyValueProvider
	{
		private readonly Regex _Regex;

		private readonly IKeyValueProvider _Inner;

		public RegexKeyValueProvider(string regex, IKeyValueProvider inner)
		{
			if (inner == null)
				throw new ArgumentNullException("inner");
			if (regex.IsNullOrWhiteSpace())
				throw new ArgumentNullException("regex");

			_Inner = inner;
			_Regex = new Regex(regex.TrimSafe());
		}

		public IEnumerable<string> Keys
		{
			get
			{
				return _Inner.Keys.Where(key =>
				{
					var match = _Regex.Match(key);
					return match.Success && match.Groups.Count > 1;
				});
			}
		}

		public string RealKey(string key)
		{
			var match = _Regex.Match(_Inner.RealKey(key));
			return match.Success && match.Groups.Count > 1
				? match.Groups[1].Value
				: null;
		}

		public string Value(string key)
		{
			return _Inner.Value(key);
		}
	}
}

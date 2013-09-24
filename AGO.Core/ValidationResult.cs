using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AGO.Core
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ValidationResult
	{
		private readonly ISet<string> _Errors = new HashSet<string>(StringComparer.InvariantCulture);
		[JsonProperty("generalErrors")]
		public IEnumerable<string> Errors { get { return _Errors; } }

		public void AddErrors(params string[] errors)
		{
			_Errors.UnionWith(FilterErrors(errors));
		}

		private readonly IDictionary<string, IEnumerable<string>> _FieldErrors =
			new Dictionary<string, IEnumerable<string>>(StringComparer.InvariantCulture);
		[JsonProperty("fieldErrors")]
		public IEnumerable<KeyValuePair<string, IEnumerable<string>>> FieldErrors { get { return _FieldErrors; } }

		public void AddFieldErrors(string field, params string[] errors)
		{
			field = field.TrimSafe();
			if (field.IsNullOrEmpty())
				throw new ArgumentNullException("field");

			if (!_FieldErrors.ContainsKey(field))
				_FieldErrors[field] = new HashSet<string>();
			((HashSet<string>)_FieldErrors[field]).UnionWith(FilterErrors(errors));
		}

		[JsonProperty("success")]
		public bool Success
		{
			get { return _Errors.Count == 0 && _FieldErrors.Count == 0; }
		}

		protected IEnumerable<string> FilterErrors(IEnumerable<string> errors)
		{
			return (errors ?? Enumerable.Empty<string>())
				.Select(s => s.TrimSafe()).Where(s => !s.IsNullOrEmpty());
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AGO.Core
{
	public class ValidationResult
	{
		[JsonProperty("generalError")]
		public string GeneralError { get; set; }

		private readonly IDictionary<string, string> _FieldErrors =
			new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		[JsonProperty("fieldErrors")]
		public IDictionary<string, string> FieldErrors { get { return _FieldErrors; } }
			
		[JsonProperty("success")]
		public bool Success
		{
			get
			{
				return GeneralError.IsNullOrWhiteSpace() &&
					(FieldErrors.Count == 0 || FieldErrors.Values.All(s => s.IsNullOrWhiteSpace()));
			}
		}
	}
}
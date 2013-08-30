using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace AGO.Core.Localization
{
	public class ResourceManagerLocalizerByKey : AbstractResourceManagerLocalizer, ILocalizerByKey
	{
		#region Properties, fields, constructors

		protected readonly StringComparison _Comparison;

		public ResourceManagerLocalizerByKey(
			string baseName, 
			Assembly assembly, 
			StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
			: base(baseName, assembly)
		{
			_Comparison = comparison;
		}

		public ResourceManagerLocalizerByKey(
			Type resourceSource,
			StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
			: base(resourceSource)
		{
			_Comparison = comparison;
		}

		#endregion

		#region Interfaces implementation

		public bool Accept(string key)
		{
			key = key.TrimSafe();
			if (key.IsNullOrEmpty())
				throw new ArgumentNullException("key");

			return _Keys.Any(k => string.Equals(key, k, _Comparison));
		}

		public string Message(string key, CultureInfo culture, object[] args)
		{
			key = key.TrimSafe();
			if (key.IsNullOrEmpty())
				throw new ArgumentNullException("key");

			return string.Format(_ResourceManager.GetString(key, culture) ?? string.Empty, args);
		}

		#endregion
	}
}
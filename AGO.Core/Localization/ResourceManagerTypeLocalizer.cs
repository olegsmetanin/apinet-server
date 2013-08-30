using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace AGO.Core.Localization
{
	public class ResourceManagerTypeLocalizer : AbstractResourceManagerLocalizer, ITypeLocalizer, ITypeLocalizerByKey
	{
		#region Constants

		public const string TypeDescriptionKey = "TypeDescription"; 

		#endregion

		#region Properties, fields, constructors	 

		protected readonly StringComparison _Comparison;

		protected readonly Type _Type;

		public ResourceManagerTypeLocalizer(
			Type type,
			StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
			: base(type)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			_Type = type;

			_Comparison = comparison;
		}

		public ResourceManagerTypeLocalizer(
			Type type,
			Type resourceSource,
			StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
			: base(resourceSource)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			_Type = type;

			_Comparison = comparison;
		}

		public ResourceManagerTypeLocalizer(
			Type type,
			string baseName,
			Assembly assembly,
			StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
			: base(baseName, assembly)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			_Type = type;

			_Comparison = comparison;
		}

		#endregion

		#region Interfaces implementation

		public bool Accept(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			return type == _Type && _Keys.Any(k => string.Equals(TypeDescriptionKey, k, _Comparison));
		}

		public string Message(Type type, CultureInfo culture, object[] args)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			return type == _Type 
				? string.Format(_ResourceManager.GetString(TypeDescriptionKey, culture) ?? 
					string.Empty, args ?? new object[0])
				: null;
		}

		public bool Accept(Type type, string key)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			key = key.TrimSafe();
			if (key.IsNullOrEmpty())
				throw new ArgumentNullException("key");

			return (_Type.IsAssignableFrom(type) || type.IsAssignableFrom(_Type))
				&& _Keys.Any(k => string.Equals(key, k, _Comparison));
		}

		public string Message(Type type, string key, CultureInfo culture, object[] args)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			
			key = key.TrimSafe();
			if (key.IsNullOrEmpty())
				throw new ArgumentNullException("key");

			return (_Type.IsAssignableFrom(type) || type.IsAssignableFrom(_Type))
				? string.Format(_ResourceManager.GetString(key, culture) ??
					string.Empty, args ?? new object[0])
				: null;
		}

		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AGO.Core.Localization
{
	public class LocalizationService : AbstractService, ILocalizationService
	{
		#region Static properties

		/*protected static IList<CultureInfo> _SatelliteAssemblyCultures = new List<CultureInfo>();
		public static IList<CultureInfo> SatelliteAssemblyCultures { get { return _SatelliteAssemblyCultures; } }*/

		#endregion

		#region Configuration properties, fields and methods

		protected LocalizationServiceOptions _Options = new LocalizationServiceOptions();
		public LocalizationServiceOptions Options
		{
			get { return _Options; }
			set { _Options = value ?? _Options; }
		}

		protected override void DoSetConfigProperty(string key, string value)
		{
			_Options.SetMemberValue(key, value);
		}

		protected override string DoGetConfigProperty(string key)
		{
			return _Options.GetMemberValue(key).ToStringSafe();
		}

		#endregion

		#region Properties, fields, constructors

		/*protected readonly IEnvironmentService _EnvironmentService;*/

		protected readonly IDictionary<CultureInfo, IList<ILocalizerByKey>> _LocalizersByKey =
			new Dictionary<CultureInfo, IList<ILocalizerByKey>>();

		protected readonly IDictionary<CultureInfo, IList<IObjectLocalizer>> _ObjectLocalizers =
			new Dictionary<CultureInfo, IList<IObjectLocalizer>>();

		protected readonly IDictionary<CultureInfo, IList<IObjectLocalizerByKey>> _ObjectLocalizersByKey =
			new Dictionary<CultureInfo, IList<IObjectLocalizerByKey>>();

		protected readonly IDictionary<CultureInfo, IList<ITypeLocalizer>> _TypeLocalizers =
			new Dictionary<CultureInfo, IList<ITypeLocalizer>>();

		protected readonly IDictionary<CultureInfo, IList<ITypeLocalizerByKey>> _TypeLocalizersByKey =
			new Dictionary<CultureInfo, IList<ITypeLocalizerByKey>>();

		protected readonly IDictionary<CultureInfo, IList<IExceptionLocalizer>> _ExceptionLocalizers =
			new Dictionary<CultureInfo, IList<IExceptionLocalizer>>();

		public LocalizationService(
			/*IEnvironmentService environmentService,*/
			IEnumerable<ILocalizerByKey> localizersByKey,
			IEnumerable<IObjectLocalizer> objectLocalizers,
			IEnumerable<IObjectLocalizerByKey> objectLocalizersByKey,
			IEnumerable<ITypeLocalizer> typeLocalizers,
			IEnumerable<ITypeLocalizerByKey> typeLocalizersByKey,
			IEnumerable<IExceptionLocalizer> exceptionLocalizers)
		{
			/*if (environmentService == null)
				throw new ArgumentNullException("environmentService");
			_EnvironmentService = environmentService;*/

			RegisterLocalizers(localizersByKey);
			RegisterLocalizers(objectLocalizers);
			RegisterLocalizers(objectLocalizersByKey);
			RegisterLocalizers(typeLocalizers);
			RegisterLocalizers(typeLocalizersByKey);
			RegisterLocalizers(exceptionLocalizers);
		}
		
		#endregion

		#region Interfaces implementation

		public IEnumerable<CultureInfo> Cultures
		{
			get
			{
				return _LocalizersByKey.Keys
					.Union(_ObjectLocalizers.Keys)
					.Union(_ObjectLocalizersByKey.Keys)
					.Union(_TypeLocalizers.Keys)
					.Union(_TypeLocalizersByKey.Keys)
					.Union(_ExceptionLocalizers.Keys);
			}
		}

		public string Message(object key, CultureInfo culture = null, params object[] args)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();
			var keyStr = key.ConvertSafe<string>().Trim();
			if (keyStr.IsNullOrEmpty())
				throw new ArgumentNullException("key");
			culture = culture ?? CultureInfo.CurrentUICulture;

			var localizer = _LocalizersByKey.ContainsKey(culture)
				? _LocalizersByKey[culture].FirstOrDefault(l => l.Accept(keyStr))
				: null;

			return localizer != null ? localizer.Message(keyStr, culture, args ?? new object[0]) : null;
		}

		public string MessageFor(object obj, object key = null, CultureInfo culture = null)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();
			if (obj == null)
				throw new ArgumentNullException("obj");
			culture = culture ?? CultureInfo.CurrentUICulture;

			var localizable = obj as ILocalizable;

			if (key == null)
			{
				var localizer = _ObjectLocalizers.ContainsKey(culture)
					? _ObjectLocalizers[culture].FirstOrDefault(l => l.Accept(obj))
					: null;

				return localizer != null 
					? localizer.Message(obj, culture)
					: MessageForType(obj.GetType(), null, culture, localizable != null ? localizable.MessageArguments.ToArray() : null);
			}

			var keyStr = key.ConvertSafe<string>().Trim();
			if (keyStr.IsNullOrEmpty())
				throw new ArgumentNullException("key");

			var localizerByKey = _ObjectLocalizersByKey.ContainsKey(culture)
				? _ObjectLocalizersByKey[culture].FirstOrDefault(l => l.Accept(obj, keyStr))
				: null;

			return localizerByKey != null
				? localizerByKey.Message(obj, keyStr, culture)
				: MessageForType(obj.GetType(), key, culture, localizable != null ? localizable.MessageArguments.ToArray() : null);
		}

		public string MessageFor<TType>(object key = null, CultureInfo culture = null, params object[] args) where TType : class
		{
			return MessageForType(typeof (TType), key, culture, args);
		}

		public string MessageForType(Type type, object key = null, CultureInfo culture = null, params object[] args)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();
			if (type == null)
				throw new ArgumentNullException("type");
			culture = culture ?? CultureInfo.CurrentUICulture;

			if (key == null)
			{
				var localizer = _TypeLocalizers.ContainsKey(culture)
					? _TypeLocalizers[culture].FirstOrDefault(l => l.Accept(type))
					: null;

				return localizer != null ? localizer.Message(type, culture, args) : null;
			}

			var keyStr = key.ConvertSafe<string>().Trim();
			if (keyStr.IsNullOrEmpty())
				throw new ArgumentNullException("key");

			var localizerByKey = _TypeLocalizersByKey.ContainsKey(culture)
				? _TypeLocalizersByKey[culture].FirstOrDefault(l => l.Accept(type, keyStr))
				: null;

			return localizerByKey != null ? localizerByKey.Message(type, keyStr, culture, args) : null;
		}

		public string MessageForException(Exception exception, CultureInfo culture = null)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();
			if (exception == null)
				throw new ArgumentNullException("exception");
			culture = culture ?? CultureInfo.CurrentUICulture;

			var localizer = _ExceptionLocalizers.ContainsKey(culture)
				? _ExceptionLocalizers[culture].FirstOrDefault(l => l.Accept(exception))
				: null;

			return localizer != null 
				? localizer.Message(exception, culture) 
				: (MessageFor(exception, null, culture) ?? MessageFor(exception, exception.GetType().Name, culture));
		}

		public void RegisterLocalizers(IEnumerable<ILocalizer> localizers)
		{
			localizers = (localizers ?? Enumerable.Empty<ILocalizer>()).ToList();

			foreach (var localizer in localizers.OfType<ILocalizerByKey>())
			{
				foreach (var culture in localizer.Cultures)
				{
					if (!_LocalizersByKey.ContainsKey(culture))
						_LocalizersByKey[culture] = new List<ILocalizerByKey>();
					_LocalizersByKey[culture].Add(localizer);
				}
			}

			foreach (var localizer in localizers.OfType<IObjectLocalizer>())
			{
				foreach (var culture in localizer.Cultures)
				{
					if (!_ObjectLocalizers.ContainsKey(culture))
						_ObjectLocalizers[culture] = new List<IObjectLocalizer>();
					_ObjectLocalizers[culture].Add(localizer);
				}
			}

			foreach (var localizer in localizers.OfType<IObjectLocalizerByKey>())
			{
				foreach (var culture in localizer.Cultures)
				{
					if (!_ObjectLocalizersByKey.ContainsKey(culture))
						_ObjectLocalizersByKey[culture] = new List<IObjectLocalizerByKey>();
					_ObjectLocalizersByKey[culture].Add(localizer);
				}
			}

			foreach (var localizer in localizers.OfType<ITypeLocalizer>())
			{
				foreach (var culture in localizer.Cultures)
				{
					if (!_TypeLocalizers.ContainsKey(culture))
						_TypeLocalizers[culture] = new List<ITypeLocalizer>();
					_TypeLocalizers[culture].Add(localizer);
				}
			}

			foreach (var localizer in localizers.OfType<ITypeLocalizerByKey>())
			{
				foreach (var culture in localizer.Cultures)
				{
					if (!_TypeLocalizersByKey.ContainsKey(culture))
						_TypeLocalizersByKey[culture] = new List<ITypeLocalizerByKey>();
					_TypeLocalizersByKey[culture].Add(localizer);
				}
			}

			foreach (var localizer in localizers.OfType<IExceptionLocalizer>())
			{
				foreach (var culture in localizer.Cultures)
				{
					if (!_ExceptionLocalizers.ContainsKey(culture))
						_ExceptionLocalizers[culture] = new List<IExceptionLocalizer>();
					_ExceptionLocalizers[culture].Add(localizer);
				}
			}
		}
		
		#endregion

		#region Template methods

		/*protected override void DoInitialize()
		{
			base.DoInitialize();

			var initializable = _EnvironmentService as IInitializable;
			if (initializable != null)
				initializable.Initialize();

			_SatelliteAssemblyCultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
				.Where(c => !c.Name.IsNullOrWhiteSpace() && (
					Directory.Exists(Path.Combine(_EnvironmentService.ApplicationAssembliesPath, c.Name)) ||
					Directory.Exists(Path.Combine(_EnvironmentService.ApplicationAssembliesPath, c.TwoLetterISOLanguageName))))
				.ToList();
		}*/
		
		#endregion
	}
}
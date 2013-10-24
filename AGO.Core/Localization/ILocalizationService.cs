using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace AGO.Core.Localization
{
	public interface ILocalizationService
	{
		IEnumerable<CultureInfo> Cultures { get; }

		string Message(object key, CultureInfo culture = null, params object[] args);

		string MessageFor<TType>(object key = null, CultureInfo culture = null, params object[] args)
			where TType : class;

		string MessageForType(Type type, object key = null, CultureInfo culture = null, params object[] args);

		string MessageFor(object obj, object key = null, CultureInfo culture = null);	

		string MessageForException(Exception exception, CultureInfo culture = null);

		void RegisterLocalizers(IEnumerable<ILocalizer> localizers);
	}

	public static class LocalizationServiceExtensions
	{
		public static void RegisterModuleLocalizers(this ILocalizationService localizationService, Assembly assembly)
		{
			if (localizationService == null)
				throw new ArgumentNullException("localizationService");
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			var localizers = new List<ILocalizer>();
			foreach (var resourceName in assembly.GetManifestResourceNames().Where(
					s => s.EndsWith(".resources")).Select(s => s.RemoveSuffix(".resources")))
			{
				var type = assembly.GetType(resourceName, false);
				if (type != null)
				{
					localizers.Add(new ResourceManagerTypeLocalizer(type));
					continue;
				}

				localizers.Add(new ResourceManagerLocalizerByKey(resourceName, assembly));
			}

			localizationService.RegisterLocalizers(localizers);
		}
	}
}
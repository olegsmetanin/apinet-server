using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AGO.Core.Localization
{
	public static class StandardLocalizersRegistrator
	{
		public static void Register(LocalizationService localizationService, params Assembly[] assemblies)
		{
			if (localizationService == null)
				throw new ArgumentNullException("localizationService");
			assemblies = assemblies ?? new Assembly[0];

			var localizers = new List<ILocalizer>();
			foreach (var assembly in assemblies)
			{
				foreach (var resourceName in assembly.GetManifestResourceNames().Where(
					s => s.EndsWith(".resources")).Select(s => s.RemoveSuffix(".resources")))
				{
					var type = assembly.GetType(resourceName, false);
					if (type != null && !type.IsValueType)
					{
						localizers.Add(new ResourceManagerTypeLocalizer(type));
						continue;
					}

					localizers.Add(new ResourceManagerLocalizerByKey(resourceName, assembly));
				}
			}

			localizationService.RegisterLocalizers(localizers);
		}
	}
}
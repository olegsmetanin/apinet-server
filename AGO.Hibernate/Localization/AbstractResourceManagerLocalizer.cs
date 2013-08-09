using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace AGO.Hibernate.Localization
{
	public abstract class AbstractResourceManagerLocalizer : ILocalizer
	{
		#region Properties, fields, constructors

		protected readonly ResourceManager _ResourceManager;

		protected readonly CultureInfo _NeutralCulture;

		protected readonly IList<string> _Keys = new List<string>();

		protected AbstractResourceManagerLocalizer(ResourceManager resourceManager)
		{
			if (resourceManager == null)
				throw new ArgumentNullException("resourceManager");
			_ResourceManager = resourceManager;

			_ResourceManager.GetString(string.Empty, CultureInfo.InvariantCulture);
			var resourceSet = _ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, false, true);
			var enumerator = resourceSet.GetEnumerator();
			while (enumerator.MoveNext())
				_Keys.Add(enumerator.Key.ConvertSafe<string>().Trim());
		}

		protected AbstractResourceManagerLocalizer(string baseName, Assembly assembly)
			: this(new ResourceManager(baseName.TrimSafe(), assembly))
		{
			_NeutralCulture = GetAssemblyNeutralCulture(assembly);
		}

		protected AbstractResourceManagerLocalizer(Type resourceSource)
			: this(new ResourceManager(resourceSource))
		{
			_NeutralCulture = GetAssemblyNeutralCulture(resourceSource.Assembly);
		}

		#endregion

		#region Interfaces implementation

		public IEnumerable<CultureInfo> Cultures
		{
			get
			{
				var result = new List<CultureInfo>() /*LocalizationService.SatelliteAssemblyCultures*/ as IEnumerable<CultureInfo>;
				
				var current = _NeutralCulture;
				while (current != null && !current.Equals(current.Parent))
				{
					result = result.Union(new[] {current});
					current = current.Parent;
				}

				return result.Union(new[] { CultureInfo.InvariantCulture });
			}
		}

		#endregion

		#region Helper methods

		protected CultureInfo GetAssemblyNeutralCulture(Assembly assembly)
		{
			var attributes = assembly.GetCustomAttributes(typeof (NeutralResourcesLanguageAttribute), false);
			var attribute = attributes.Length > 0 ? attributes[0] as NeutralResourcesLanguageAttribute : null;

			return attribute != null ? CultureInfo.CreateSpecificCulture(attribute.CultureName) : null;
		}

		#endregion
	}
}
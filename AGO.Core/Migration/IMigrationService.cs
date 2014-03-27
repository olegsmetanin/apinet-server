using System;
using System.Collections.Generic;
using System.Reflection;

namespace AGO.Core.Migration
{
	public interface IMigrationService
	{
		void MigrateUp(string provider, string connectionString, IEnumerable<Assembly> assemblies, string[] tags, Version upToVersion = null, bool previewOnly = false);

		void MigrateDown(string provider, string connectionString, IEnumerable<Assembly> assemblies, string[] tags, Version downToVersion = null, bool previewOnly = false);
	}
}

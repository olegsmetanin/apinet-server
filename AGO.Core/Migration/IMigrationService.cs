using System;

namespace AGO.Core.Migration
{
	public interface IMigrationService
	{
		void MigrateUp(Version upToVersion = null, bool previewOnly = false);

		void MigrateDown(Version downToVersion = null, bool previewOnly = false);
	}
}

using System;
using System.Collections.Generic;
using AGO.Core.Filters.Metadata;
using NHibernate;

namespace AGO.Core.DataAccess
{
	public interface ISessionProvider
	{
		ISession CurrentSession { get; }

		void FlushCurrentSession(bool forceRollback = false);

		void CloseCurrentSession(bool forceRollback = false);

		ISessionFactory SessionFactory { get; }

		string ConnectionString { get; }

		IEnumerable<IModelMetadata> AllModelsMetadata { get; }

		IModelMetadata ModelMetadata(Type modelType);
	}
}

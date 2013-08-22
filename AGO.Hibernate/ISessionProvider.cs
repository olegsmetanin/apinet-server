using System;
using System.Collections.Generic;
using NHibernate;
using AGO.Hibernate.Filters.Metadata;

namespace AGO.Hibernate
{
	public interface ISessionProvider
	{
		ISession CurrentSession { get; }

		void CloseCurrentSession(bool forceRollback = false);

		ISessionFactory SessionFactory { get; }

		IEnumerable<IModelMetadata> AllModelsMetadata { get; }

		IModelMetadata ModelMetadata(Type modelType);
	}
}

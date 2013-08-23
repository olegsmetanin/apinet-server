using System;

namespace AGO.Hibernate.Modules
{
	public interface IServiceDescriptor
	{
		IModuleDescriptor Module { get; }

		Type ServiceType { get; }

		string Name { get; }

		int Priority { get; }

		void Register(IModuleConsumer consumer);
	}
}

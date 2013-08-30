using System;

namespace AGO.Core.Modules
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

using System;
using AGO.Core.Application;

namespace AGO.Core.Modules
{
	public interface IServiceDescriptor
	{
		IModuleDescriptor Module { get; }

		Type ServiceType { get; }

		string Name { get; }

		int Priority { get; }

		void Register(IApplication app);

		void Initialize(IApplication app);
	}
}

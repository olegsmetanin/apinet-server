using System.Collections.Generic;
using AGO.Core.Application;

namespace AGO.Core.Modules
{
	public interface IModuleDescriptor
	{
		string Name { get; }

		string Alias { get; }

		int Priority { get; }

		IEnumerable<IServiceDescriptor> Services { get; }

		void Register(IApplication app);

		void Initialize(IApplication app);
	}
}
using System.Collections.Generic;
using AGO.Core.Modules;
using AGO.Core.Controllers;
using AGO.Core.Validation;

namespace AGO.Core
{
	public class ModuleDescriptor : IModuleDescriptor
	{
		public string Name { get { return "AGO.Core"; } }

		public string Alias { get { return "Core"; } }

		public int Priority { get { return int.MinValue; } }

		public IEnumerable<IServiceDescriptor> Services { get; private set; }

		public void Register(IModuleConsumer consumer)
		{
			consumer.RegisterModelValidator<AttributeValidatingModelValidator>();
		}

		public ModuleDescriptor()
		{
			Services = new List<IServiceDescriptor>
			{
				new AttributedServiceDescriptor<DictionaryController>(this),
				new AttributedServiceDescriptor<AuthController>(this)
			};
		}
	}
}

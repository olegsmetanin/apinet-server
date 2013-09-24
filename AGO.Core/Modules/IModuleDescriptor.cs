using System.Collections.Generic;
using AGO.Core.Validation;

namespace AGO.Core.Modules
{
	public interface IModuleDescriptor
	{
		string Name { get; }

		string Alias { get; }

		int Priority { get; }

		IEnumerable<IServiceDescriptor> Services { get; }

		void Register(IModuleConsumer consumer);
	}

	public static class ModuleDescriptorExtensions
	{
		public static void RegisterModelValidator<TType>(this IModuleConsumer consumer)
			where TType: class, IModelValidator
		{
			if (consumer == null)
				return;

			consumer.Container.RegisterSingle<TType, TType>();
			consumer.Container.RegisterInitializer<TType>(validator =>
				consumer.Container.GetInstance<IValidationService>().RegisterModelValidator(validator));
		}
	}
}
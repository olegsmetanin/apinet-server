using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Controllers;
using AGO.Core.Execution;

namespace AGO.Core.Application
{
	public abstract class AbstractControllersApplication : AbstractPersistenceApplication, IControllersApplication
	{
		#region Properties, fields, constructors

		protected IStateStorage<object> _StateStorage;
		public IStateStorage<object> StateStorage { get { return _StateStorage; } }

		protected IStateStorage<string> _ClientStateStorage;
		public IStateStorage<string> ClientStateStorage { get { return _ClientStateStorage; } }

		#endregion

		#region Template methods

		protected override void DoRegisterCoreServices()
		{
			base.DoRegisterCoreServices();

			DoRegisterStateManagement();
			DoRegisterActionExecution();
		}

		protected virtual void DoRegisterStateManagement()
		{
			IocContainer.RegisterSingle<IStateStorage<object>, DictionaryStateStorage<object>>();
			IocContainer.RegisterSingle<IStateStorage<string>, DictionaryStateStorage<string>>();			
		}

		protected virtual void DoRegisterActionExecution()
		{
			IocContainer.RegisterAll<IActionParameterResolver>(AllActionParameterResolvers);
			IocContainer.RegisterAll<IActionParameterTransformer>(AllActionParameterTransformers.Concat(
				new[] { typeof(AttributeValidatingParameterTransformer) }));
			IocContainer.RegisterAll<IActionResultTransformer>(AllActionResultTransformers);

			IocContainer.RegisterSingle<IActionExecutor, ActionExecutor>();
		}

		protected virtual IEnumerable<Type> AllActionParameterResolvers
		{
			get { return Enumerable.Empty<Type>(); }
		}

		protected virtual IEnumerable<Type> AllActionParameterTransformers
		{
			get
			{
				return new[]
				{
					typeof(FilterParameterTransformer),
					typeof(JsonTokenParameterTransformer)
				};
			}
		}

		protected virtual IEnumerable<Type> AllActionResultTransformers
		{
			get { return Enumerable.Empty<Type>(); }
		}

		protected override void DoInitializeApplication()
		{
			base.DoInitializeApplication();

			DoInitializeStateManagement();
		}

		protected virtual void DoInitializeStateManagement()
		{
			_StateStorage = IocContainer.GetInstance<IStateStorage<object>>();
			_ClientStateStorage = IocContainer.GetInstance<IStateStorage<string>>();
		}

		#endregion
	}
}
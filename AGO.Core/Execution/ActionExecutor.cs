using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AGO.Core.Execution
{
	public class ActionExecutor : IActionExecutor
	{
		#region Properties, fields, constructors

		protected readonly IList<IActionParameterResolver> _Resolvers;

		protected readonly IList<IActionParameterTransformer> _Transformers;

		public ActionExecutor(
			IEnumerable<IActionParameterResolver> resolvers,
			IEnumerable<IActionParameterTransformer> transformers)
		{
			if (resolvers == null)
				throw new ArgumentNullException("resolvers");
			_Resolvers = new List<IActionParameterResolver>(resolvers);

			if (transformers == null)
				throw new ArgumentNullException("transformers");
			_Transformers = new List<IActionParameterTransformer>(transformers);
		}

		#endregion

		#region Interfaces implementation

		public object Execute(object callee, MethodInfo methodInfo)
		{
			if (callee == null)
				throw new ArgumentNullException("callee");
			if (methodInfo == null || methodInfo.DeclaringType == null)
				throw new ArgumentNullException("methodInfo");
			try
			{
				if (methodInfo.DeclaringType.IsInterface)
					methodInfo = callee.GetType().GetMethod(methodInfo.Name) ?? methodInfo;

				var parametersInfo = methodInfo.GetParameters();
				var transformedParams = new object[parametersInfo.Length];

				foreach (var info in parametersInfo)
				{
					var parameterInfo = info;
					transformedParams[parameterInfo.Position] = null;

					object resolvedParam = null;
					foreach (var parameterResolver in _Resolvers.Where(
						parameterResolver => parameterResolver.Accepts(parameterInfo)))
					{
						if (!parameterResolver.Resolve(parameterInfo, out resolvedParam))
							continue;
						break;
					}

					foreach (var transformer in _Transformers.Where(
						parameterTransformer => parameterTransformer.Accepts(parameterInfo, resolvedParam)))
					{
						var transformedParam = resolvedParam;
						if (!transformer.Transform(parameterInfo, ref transformedParam))
							continue;
						transformedParams[parameterInfo.Position] = transformedParam;
						break;
					}
				}

				var result = methodInfo.Invoke(callee, transformedParams);

				return result;
			}
			catch (TargetInvocationException e)
			{
				throw e.InnerException;
			}
		}

		#endregion
	}
}
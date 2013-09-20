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

		protected readonly IList<IActionResultTransformer> _ResultTransformers;

		public ActionExecutor(
			IEnumerable<IActionParameterResolver> resolvers,
			IEnumerable<IActionParameterTransformer> transformers,
			IEnumerable<IActionResultTransformer> resultTransformers)
		{
			if (resolvers == null)
				throw new ArgumentNullException("resolvers");
			_Resolvers = new List<IActionParameterResolver>(resolvers);

			if (transformers == null)
				throw new ArgumentNullException("transformers");
			_Transformers = new List<IActionParameterTransformer>(transformers);

			if (resultTransformers == null)
				throw new ArgumentNullException("resultTransformers");
			_ResultTransformers = new List<IActionResultTransformer>(resultTransformers);
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
				var resolvedParams = new object[parametersInfo.Length];

				foreach (var info in parametersInfo)
				{
					var parameterInfo = info;
					resolvedParams[parameterInfo.Position] = null;

					object resolvedParam = null;
					foreach (var parameterResolver in _Resolvers.Where(
						parameterResolver => parameterResolver.Accepts(parameterInfo)))
					{
						if (!parameterResolver.Resolve(parameterInfo, out resolvedParam))
							continue;
						break;
					}

					foreach (var transformer in _Transformers)
					{
						if (!transformer.Accepts(parameterInfo, resolvedParam))
							continue;
						resolvedParam = transformer.Transform(parameterInfo, resolvedParam);
					}

					resolvedParams[parameterInfo.Position] = resolvedParam;
				}

				var result = methodInfo.Invoke(callee, resolvedParams);

				foreach (var transformer in _ResultTransformers)
				{
					if (!transformer.Accepts(methodInfo.ReturnType, result))
						continue;
					result = transformer.Transform(methodInfo.ReturnType, result);
				}

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
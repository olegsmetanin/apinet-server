using System.Reflection;

namespace AGO.Core.Execution
{
	public interface IActionParameterResolver
	{
		bool Accepts(ParameterInfo parameterInfo);

		bool Resolve(ParameterInfo parameterInfo, out object parameterValue);
	}
}
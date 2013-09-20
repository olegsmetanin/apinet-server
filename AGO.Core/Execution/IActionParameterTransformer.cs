using System.Reflection;

namespace AGO.Core.Execution
{
	public interface IActionParameterTransformer
	{
		bool Accepts(ParameterInfo parameterInfo, object parameterValue);

		object Transform(ParameterInfo parameterInfo, object parameterValue);
	}
}
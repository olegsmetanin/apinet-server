using System.Reflection;

namespace AGO.Core.Execution
{
	public interface IActionParameterTransformer
	{
		bool Accepts(ParameterInfo parameterInfo, object parameterValue);

		bool Transform(ParameterInfo parameterInfo, ref object parameterValue);
	}
}
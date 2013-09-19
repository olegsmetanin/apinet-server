using System.Reflection;

namespace AGO.Core.Execution
{
	public interface IActionExecutor
	{
		object Execute(object callee, MethodInfo methodInfo);
	}
}
using System;

namespace AGO.Core.Execution
{
	public interface IActionResultTransformer
	{
		bool Accepts(Type returnType, object returnValue);

		object Transform(Type returnType, object returnValue);
	}
}
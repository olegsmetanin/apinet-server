using System;

namespace AGO.Core.Execution
{
	public abstract class AbstractExecutionException : AbstractApplicationException
	{
		protected AbstractExecutionException(string message = null, Exception innerException = null)
			: base(message, innerException)
		{
		}
	}

	public class ControllerActionParameterException : AbstractExecutionException
	{
		public ControllerActionParameterException(string parameter, Exception innerException = null)
			: base(null, innerException)
		{
			_MessageArguments.Add(parameter);
		}
	}
}

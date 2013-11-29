using System;
using AGO.Core;

namespace AGO.Reporting.Service
{
	public class ReportingException: AbstractApplicationException
	{
		public ReportingException(string message = null, Exception innerException = null)
			: base(message, innerException)
		{
		}
	}
}
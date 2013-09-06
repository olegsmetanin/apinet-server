using System;

namespace AGO.Home
{
	public class NoInitialProjectStatusException : Exception
	{
		public NoInitialProjectStatusException()
			: base("No initial project status in dictionary")
		{
		}
	}
}

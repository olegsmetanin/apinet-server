using System;
using AGO.Core;
using NHibernate;

namespace AGO.Reporting.Service
{
	/// <summary>
	/// For CurrentSessionContext with ThreadStatic binding important to close
	/// current session after every persistance action, because session binded to thread pool thread
	/// may be used in other Task/Timer/api request and provide side effects. 
	/// </summary>
	internal static class DALHelper
	{
		internal static void Do(ISessionProvider provider, Action<ISession> action)
		{
			try
			{
				action(provider.CurrentSession);
			}
			finally
			{
				provider.CloseCurrentSession();
			}
		}

		internal static TResult Do<TResult>(ISessionProvider provider, Func<ISession, TResult> action)
		{
			try
			{
				return action(provider.CurrentSession);
			}
			finally
			{
				provider.CloseCurrentSession();
			}
		}
	}
}
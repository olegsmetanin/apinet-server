using System;
using AGO.Core;
using Common.Logging;
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
			catch (OperationCanceledException)
			{
				//this is expected situation
			}
			catch(Exception ex)
			{
				LogManager.GetLogger(typeof(DALHelper)).Error("Error when accessing database from report service", ex);					
			}
			finally
			{
				provider.CloseCurrentSession();
			}
		}

		internal static TResult Do<TResult>(ISessionProvider provider, Func<ISession, TResult> action)
		{
			var result = default(TResult);
			Do(provider, s => { result = action(s); });
			return result;
		}
	}
}
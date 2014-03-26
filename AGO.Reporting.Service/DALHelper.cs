using System;
using AGO.Core.DataAccess;
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
		internal static void Do(ISessionProviderRegistry registry, string project, Action<ISession, ISession> action)
		{
			var mainProvider = registry.GetMainDbProvider();
			var projProvider = registry.GetProjectProvider(project);
			try
			{
				action(mainProvider.CurrentSession, projProvider.CurrentSession);
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
				registry.CloseCurrentSessions();
			}
		}

		internal static TResult Do<TResult>(ISessionProviderRegistry registry, string project, Func<ISession, ISession, TResult> action)
		{
			var result = default(TResult);
			Do(registry, project, (ms, ps) => { result = action(ms, ps); });
			return result;
		}
	}
}
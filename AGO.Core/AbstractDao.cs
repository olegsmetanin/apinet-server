using System;
using NHibernate;

namespace AGO.Core
{
	public abstract class AbstractDao : AbstractService
	{
		#region Properties, fields, constructors

		protected ISessionProvider _SessionProvider;

		protected ISession CurrentSession { get { return _SessionProvider.CurrentSession; } }

		protected AbstractDao(ISessionProvider sessionProvider)
		{
			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");
			_SessionProvider = sessionProvider;
		}

		#endregion

		#region Template methods

		protected override void DoInitialize()
		{
			base.DoInitialize();

			_SessionProvider.TryInitialize();
		}

		#endregion
	}
}

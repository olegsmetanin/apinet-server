using System;
using System.Globalization;
using NHibernate;

namespace AGO.Core
{
	public abstract class AbstractDao : AbstractService
	{
		#region Constants

		private const int MaxPageSize = 100;

		private const int DefaultPageSize = 10;

		#endregion

		#region Configuration properties, fields and methods

		protected int _MaxPageSize = MaxPageSize;

		protected int _DefaultPageSize = DefaultPageSize;

		protected override void DoSetConfigProperty(string key, string value)
		{
			if ("MaxPageSize".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				_MaxPageSize = value.ConvertSafe<int>();
			if ("DefaultPageSize".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				_DefaultPageSize = value.ConvertSafe<int>();
		}

		protected override string DoGetConfigProperty(string key)
		{
			if ("MaxPageSize".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				return _MaxPageSize.ToString(CultureInfo.InvariantCulture);
			if ("DefaultPageSize".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				return _DefaultPageSize.ToString(CultureInfo.InvariantCulture);
			return null;
		}

		#endregion

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

		protected override void DoFinalizeConfig()
		{
			base.DoFinalizeConfig();

			if (_MaxPageSize <= 0)
				_MaxPageSize = MaxPageSize;
			if (_DefaultPageSize <= 0)
				_DefaultPageSize = DefaultPageSize;
			if (_DefaultPageSize < _MaxPageSize)
				_DefaultPageSize = _MaxPageSize;
		}

		protected override void DoInitialize()
		{
			base.DoInitialize();

			_SessionProvider.TryInitialize();
		}

		#endregion
	}
}

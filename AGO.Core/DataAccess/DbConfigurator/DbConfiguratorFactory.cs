using System;
using System.Collections.Generic;

namespace AGO.Core.DataAccess.DbConfigurator
{
	public class DbConfiguratorFactory
	{
		private readonly IDictionary<string, Func<string, IDbConfigurator>> factories = 
			new Dictionary<string, Func<string, IDbConfigurator>>
		{
			{"System.Data.SqlClient", cs => { throw new NotImplementedException(); }},
			{"PostgreSQL", cs => new PostgresConfigurator(cs)}
		};

		public IDbConfigurator CreateConfigurator(string provider, string masterConnectionString)
		{
			if (provider.IsNullOrWhiteSpace())
				throw new ArgumentNullException("provider");
			if (masterConnectionString.IsNullOrWhiteSpace())
				throw new ArgumentNullException("masterConnectionString");

			if (!factories.ContainsKey(provider))
				throw new InvalidOperationException(string.Format("Unsupported db configurator provider: {0}", provider));

			return factories[provider](masterConnectionString);
		}
	}
}

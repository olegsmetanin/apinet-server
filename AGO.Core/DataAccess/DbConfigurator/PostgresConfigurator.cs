using System.Data;
using Npgsql;

namespace AGO.Core.DataAccess.DbConfigurator
{
	public class PostgresConfigurator: AbstractDbConfigurator
	{
		public PostgresConfigurator(string connectionString) : base(connectionString)
		{
		}

		public override string MakeConnectionString(string host, string database, string template = null)
		{
			var builder = new NpgsqlConnectionStringBuilder(template ?? MasterConnectionString);
			builder.Host = host ?? builder.Host;
			builder.Database = database ?? builder.Database;
			return builder.ConnectionString;
		}

		protected override IDbConnection MakeDbConnection(string connectionString)
		{
			return new NpgsqlConnection(connectionString);
		}

		protected override string MakeDbCreationScript(string dbName)
		{
			return string.Format(@"
				select pg_terminate_backend(pg_stat_activity.pid) from pg_stat_activity where datname = '{0}';
				go
				drop database if exists {0};
				go
				create database {0};", dbName);
		}

		protected override string MakeLoginCreationScript(string login, string pwd)
		{
			return string.Format(@"
				DO
				$body$
				begin
					if not exists (select * from pg_catalog.pg_user where usename = '{0}') then
						create role {0} login password '{1}';
					end if;
				end
				$body$", login, pwd);
		}

		protected override string MakeConfigureDatabaseScript(string dbName, string login)
		{
			return string.Format("alter database {0} owner to {1}", dbName, login);
		}

		private void InstallExtensions(string host, string dbName)
		{
			using (var conn = MakeDbConnection(MakeConnectionString(host, dbName)))
			{
				conn.Open();
				ExecuteNonQuery("create extension if not exists citext with schema public", conn);
				conn.Close();
			}
		}

		public override void CreateMasterDatabase(string dbName, string owner, string ownerpwd)
		{
			base.CreateMasterDatabase(dbName, owner, ownerpwd);
			InstallExtensions(null, dbName);
		}

		public override void CreateProjectDatabase(string host, string dbName, string owner, string ownerpwd)
		{
			base.CreateProjectDatabase(host, dbName, owner, ownerpwd);
			InstallExtensions(host, dbName);
		}
	}
}

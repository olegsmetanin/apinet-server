using System;
using Npgsql;

namespace AGO.WorkQueue.Tests
{
	public static class PostgreSqlHelper
	{
		private const string MasterConnStr = "Server=localhost; Database=postgres; User ID=postgres; Password=postgres;";
		public const string ConnStr = "Server=localhost; Database=ago_apinet_workqueue_nunit; User ID=nunit_user; Password=123";
		private const string CmdCreate = @"
select pg_terminate_backend(pg_stat_activity.pid) from pg_stat_activity where datname = '{0}';
go
drop database if exists {0};
go
drop role if exists {1};
go
create database {0};
go
create role {1} login password '{2}';
go
alter database {0} owner to {1};";
		private const string CmdCreateQueue = @"
create table work_queue(
	task_type character varying(128) not null, 
	task_id uuid not null, 
	project character varying(64) not null,
	user_id uuid not null,
	cdate timestamp without time zone not null,
	priority_type integer not null,
	user_priority integer not null,
	constraint pk_work_queue primary key (task_id)
);
go
alter table work_queue owner to {0};";
		private const string CmdDrop = @"
select pg_terminate_backend(pg_stat_activity.pid) from pg_stat_activity where datname = '{0}';
go
drop database if exists {0};
go
drop role if exists {1};";

		public static void CreateDbAndSchema()
		{
			ExecuteBatch(MasterConnStr, string.Format(CmdCreate, "ago_apinet_workqueue_nunit", "nunit_user", "123"));
			ExecuteBatch(ConnStr, string.Format(CmdCreateQueue, "nunit_user"));
		}

		public static void DropDb()
		{
			ExecuteBatch(MasterConnStr, string.Format(CmdDrop, "ago_apinet_workqueue_nunit", "nunit_user"));
		}

		public static void ExecuteBatch(string cs, string batch)
		{
			using(var conn = new NpgsqlConnection(cs))
			{
				conn.Open();

				var cmd = conn.CreateCommand();
				foreach (var sql in batch.Split(new []{"go\r\n"}, StringSplitOptions.RemoveEmptyEntries))
				{
					cmd.CommandText = sql;
					cmd.ExecuteNonQuery();
				}

				conn.Close();
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Npgsql;

namespace AGO.WorkQueue
{
	public class PostgreSqlWorkQueue: IWorkQueue
	{
		private readonly string connectionString;
		private readonly Schema schema;

		public PostgreSqlWorkQueue(string connStr, Schema schema = null)
		{
			if (string.IsNullOrWhiteSpace(connStr))
				throw new ArgumentNullException("connStr");

			connectionString = connStr;
			this.schema = schema ?? DefaultSchema;
		}

		private T Exec<T>(Func<DbConnection, T> action)
		{
			T result;
			using (var conn = new NpgsqlConnection(connectionString))
			{
				conn.Open();
				result = action(conn);
				conn.Close();
			}
			return result;
		}

		public void Add(QueueItem item)
		{
			Exec<object>(connection =>
			{
				var cmd = connection.CreateCommand();
				cmd.CommandText = BuildQuery(AddSql);

				var ptt = cmd.CreateParameter();
				ptt.ParameterName = "tt";
				ptt.DbType = DbType.String;
				ptt.Value = item.TaskType;

				var ptid = cmd.CreateParameter();
				ptid.ParameterName = "tid";
				ptid.DbType = DbType.Guid;
				ptid.Value = item.TaskId;

				var pproj = cmd.CreateParameter();
				pproj.ParameterName = "proj";
				pproj.DbType = DbType.String;
				pproj.Value = item.Project;

				var puid = cmd.CreateParameter();
				puid.ParameterName = "uid";
				puid.DbType = DbType.Guid;
				puid.Value = item.UserId;

				var pcdate = cmd.CreateParameter();
				pcdate.ParameterName = "cdate";
				pcdate.DbType = DbType.DateTime;
				pcdate.Value = item.CreateDate;

				var pptype = cmd.CreateParameter();
				pptype.ParameterName = "ptype";
				pptype.DbType = DbType.Int32;
				pptype.Value = item.PriorityType;

				var pup = cmd.CreateParameter();
				pup.ParameterName = "priority";
				pup.DbType = DbType.Int32;
				pup.Value = item.UserPriority;

				cmd.Parameters.Add(ptt);
				cmd.Parameters.Add(ptid);
				cmd.Parameters.Add(pproj);
				cmd.Parameters.Add(puid);
				cmd.Parameters.Add(pcdate);
				cmd.Parameters.Add(pptype);
				cmd.Parameters.Add(pup);

				cmd.ExecuteNonQuery();

				return null;
			});
		}

		public QueueItem Get(string project)
		{
			return Exec(connection =>
			{
				var cmd = connection.CreateCommand();
				cmd.CommandText = BuildQuery(GetByUserPriority);
				var pproj = cmd.CreateParameter();
				pproj.ParameterName = "project";
				pproj.DbType = DbType.String;
				pproj.Value = project;
				cmd.Parameters.Add(pproj);

				using (var r = cmd.ExecuteReader())
				{
					if (r.Read())
					{
						return Map(r);
					}
					r.Close();
				}

				cmd.CommandText = BuildQuery(GetByDate);
				using (var r = cmd.ExecuteReader())
				{
					if (r.Read())
					{
						return Map(r);
					}
					r.Close();
				}

				return null;
			});
		}

		private static QueueItem Map(DbDataReader r)
		{
			return new QueueItem(r.GetString(0), r.GetGuid(1), r.GetString(2), r.GetGuid(3), r.GetDateTime(4))
			{
				PriorityType = r.GetInt32(5),
				UserPriority = r.GetInt32(6)
			};
		}

		public IEnumerable<string> UniqueProjects()
		{
			return Exec(connection =>
			{
				var cmd = connection.CreateCommand();
				cmd.CommandText = BuildQuery(ListProjectsSql);
				var projects = new List<string>();
				using (var r = cmd.ExecuteReader())
				{
					while (r.Read())
					{
						projects.Add(r.GetString(0));
					}
					r.Close();
				}
				return projects;
			});
		}

		public IEnumerable<QueueItem> Dump()
		{
			return Exec(connection =>
			{
				var cmd = connection.CreateCommand();
				cmd.CommandText = BuildQuery(GetRaw);
				var queue = new List<QueueItem>();
				using (var r = cmd.ExecuteReader())
				{
					while (r.Read())
					{
						queue.Add(Map(r));
					}
					r.Close();
				}
				return queue;
			});
		}

		public IDictionary<Guid, IDictionary<string, QueueItem[]>> Snapshot()
		{
			//farst dump from db
			var queue = Dump().ToList();
			//and numerate consistent data without blocking db (row count usually must be very small)

			//first, split by projects, order by priority algorithm and assign order number in project
			var mapByProject = queue
					.Select(i => i.Project).Distinct()
					.ToDictionary(p => p, p =>
					{
						var withPriority = from q in queue where q.Project == p && q.PriorityType > 0
										   orderby q.PriorityType descending, q.UserPriority descending, q.CreateDate
										   select q;

						var withoutPriority = from q in queue where q.Project == p && q.PriorityType == 0
											  orderby q.CreateDate select q;

						return withPriority.Concat(withoutPriority).Select((i, index) =>
						{
							i.OrderInQueue = index + 1;
							return i;
						}).ToList();
					});

			//then split by user and for each reduce project map only for this user tasks
			return queue
				.Select(i => i.UserId).Distinct()
				.ToDictionary<Guid, Guid, IDictionary<string, QueueItem[]>>(uid => uid,
					uid => mapByProject
							.Where(kvp => kvp.Value.Any(i => i.UserId == uid))
							.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Where(i => i.UserId == uid).ToArray()));
		}

		public void Clear()
		{
			Exec<object>(connection =>
			{
				var cmd = connection.CreateCommand();
				cmd.CommandText = BuildQuery(ClearSql);
				cmd.ExecuteNonQuery();

				return null;
			});
		}

		#region Table schema

		/// <summary>
		/// Schema for queue table
		/// </summary>
		public sealed class Schema
		{
			public string Table { get; set; }
			public string TaskTypeColumn { get; set; }
			public string TaskIdColumn { get; set; }
			public string ProjectColumn { get; set; }
			public string UserIdColumn { get; set; }
			public string CreateDateColumn { get; set; }
			public string PriorityTypeColumn { get; set; }
			public string UserPriorityColumn { get; set; }
		}

		public static readonly Schema DefaultSchema = new Schema
		{
			Table = "work_queue",
			TaskTypeColumn = "task_type",
			TaskIdColumn = "task_id",
			ProjectColumn = "project",
			UserIdColumn = "user_id",
			CreateDateColumn = "cdate",
			PriorityTypeColumn = "priority_type",
			UserPriorityColumn = "user_priority"
		};

		#endregion

		#region Query builder

		private const string PhTable = "$table$";
		private const string PhTaskType = "$taskType$";
		private const string PhTaskId = "$taskId$";
		private const string PhProject = "$project$";
		private const string PhUserId = "$userId$";
		private const string PhCreateDate = "$cdate$";
		private const string PhPriorityType = "$ptype$";
		private const string PhUserPriority = "$priority$";
		private const string PhColumns = PhTaskType + ", " + PhTaskId + ", " + PhProject + ", " + PhUserId + ", " +
		                                 PhCreateDate + ", " + PhPriorityType + ", " + PhUserPriority;
		private const string ClearSql = "truncate " + PhTable;
		private const string AddSql = "insert into " + PhTable + "(" + PhColumns + ") values(:tt, :tid, :proj, :uid, :cdate, :ptype, :priority)";
		private const string ListProjectsSql = "select distinct " + PhProject + " from " + PhTable;
		private const string GetByUserPriority = "delete from " + PhTable + " where " + PhTaskId +
		                                         "=(select " + PhTaskId + " from " + PhTable + " where " +
		                                         PhProject + "=:project and " +
		                                         PhPriorityType + ">0 " +
		                                         "order by " + PhPriorityType + " desc, " + PhUserPriority + " desc, " + PhCreateDate + " asc " +
		                                         "limit 1 for update) returning " + PhColumns;
		private const string GetByDate = "delete from " + PhTable + " where " + PhTaskId +
												 "=(select " + PhTaskId + " from " + PhTable + " where " +
												 PhProject + "=:project and " +
												 PhPriorityType + "=0 " +
												 "order by " + PhCreateDate + " asc " +
												 "limit 1 for update) returning " + PhColumns;
		private const string GetRaw = "select " + PhColumns + " from " + PhTable;


		private string BuildQuery(string template)
		{
			return template
				.Replace(PhTable, schema.Table)
				.Replace(PhTaskType, schema.TaskTypeColumn)
				.Replace(PhTaskId, schema.TaskIdColumn)
				.Replace(PhProject, schema.ProjectColumn)
				.Replace(PhUserId, schema.UserIdColumn)
				.Replace(PhCreateDate, schema.CreateDateColumn)
				.Replace(PhPriorityType, schema.PriorityTypeColumn)
				.Replace(PhUserPriority, schema.UserPriorityColumn);
		}

		#endregion
	}
}
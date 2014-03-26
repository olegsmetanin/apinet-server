using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Common.Logging;

namespace AGO.Core.DataAccess.DbConfigurator
{
	public abstract class AbstractDbConfigurator: IDbConfigurator
	{
		protected readonly ILog Log;

		protected AbstractDbConfigurator(string connectionString)
		{
			if (connectionString.IsNullOrWhiteSpace())
				throw new ArgumentNullException("connectionString");

			MasterConnectionString = connectionString;
			Log = LogManager.GetLogger(GetType());
		}

		public string MasterConnectionString { get; private set; }

		public abstract string MakeConnectionString(string host, string database, string template = null);

		protected abstract IDbConnection MakeDbConnection(string connectionString);

		protected abstract string MakeDbCreationScript(string dbName);

		protected abstract string MakeLoginCreationScript(string login, string pwd);

		protected abstract string MakeConfigureDatabaseScript(string dbName, string login);

		public virtual void CreateMasterDatabase(string dbName, string owner, string ownerpwd)
		{
			using (var conn = MakeDbConnection(MasterConnectionString))
			{
				conn.Open();

				ExecuteNonQuery(MakeDbCreationScript(dbName), conn);
				ExecuteNonQuery(MakeLoginCreationScript(owner, ownerpwd), conn);
				ExecuteNonQuery(MakeConfigureDatabaseScript(dbName, owner), conn);

				conn.Close();
			}
		}

		public virtual void CreateProjectDatabase(string host, string dbName, string owner, string ownerpwd)
		{
			var projMasterCs = MakeConnectionString(host, null);
			using (var conn = MakeDbConnection(projMasterCs))
			{
				conn.Open();

				ExecuteNonQuery(MakeDbCreationScript(dbName), conn);
				ExecuteNonQuery(MakeLoginCreationScript(owner, ownerpwd), conn);
				ExecuteNonQuery(MakeConfigureDatabaseScript(dbName, owner), conn);

				conn.Close();
			}
		}

		protected void ExecuteNonQuery(string script, IDbConnection connection)
		{
			var scripts = new List<string>();
			using (var reader = new StringReader(script))
			{
				var line = reader.ReadLine();
				var currentBatch = new StringBuilder();
				while (line != null)
				{
					line = line.TrimSafe();
					if (!"GO".Equals(line, StringComparison.InvariantCultureIgnoreCase))
						currentBatch.AppendLine(line);
					else
					{
						if (currentBatch.Length > 0)
							scripts.Add(currentBatch.ToString());
						currentBatch = new StringBuilder();
					}

					line = reader.ReadLine();
				}

				if (currentBatch.Length > 0)
					scripts.Add(currentBatch.ToString());
			}

			foreach (var str in scripts)
			{
				var command = connection.CreateCommand();
				command.CommandText = str;
				command.CommandType = CommandType.Text;

				var rowsAffected = command.ExecuteNonQuery();
				if (rowsAffected >= 0)
					Console.WriteLine("Rows affected: {0}", rowsAffected);
			}

			Log.InfoFormat("Batch complete ({0} scripts executed)", scripts.Count);
		}
	}
}

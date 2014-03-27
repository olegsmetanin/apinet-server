using System.Data;
using NHibernate.Dialect;

namespace AGO.Core.DataAccess
{
	//Not used but may be needed later in some queres that use direct cast in case clause (see usage of GetTypeName method from base class)
	public class PostgreSqlCaseInsensitiveDialect: PostgreSQL82Dialect
	{
		public PostgreSqlCaseInsensitiveDialect()
		{
			RegisterColumnType(DbType.AnsiStringFixedLength, "citext");
			RegisterColumnType(DbType.AnsiStringFixedLength, 8000, "citext");
			RegisterColumnType(DbType.AnsiString, "citext");
			RegisterColumnType(DbType.AnsiString, 8000, "citext");
			RegisterColumnType(DbType.AnsiString, int.MaxValue, "citext");
			RegisterColumnType(DbType.StringFixedLength, "citext");
			RegisterColumnType(DbType.StringFixedLength, 4000, "citext");
			RegisterColumnType(DbType.String, "citext");
			RegisterColumnType(DbType.String, 4000, "citext");
			RegisterColumnType(DbType.String, 1073741823, "citext");
		}
	}
}
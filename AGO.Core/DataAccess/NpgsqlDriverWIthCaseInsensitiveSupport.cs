using System.Data;
using NHibernate.Driver;
using NHibernate.SqlTypes;

namespace AGO.Core.DataAccess
{
	/// <summary>
	/// Because postgresql cast left side of predicate to text, if rigth side is text (and rigth side is parameter typycally)
	/// we need drop explicit type from this parameters, so, postgresql can cast rigth side to citext instead of text.
	/// This driver set in connection.driver_class config key of nhibernate
	/// </summary>
	public class NpgsqlDriverWithCaseInsensitiveSupport: NpgsqlDriver
	{
		protected override void InitializeParameter(IDbDataParameter dbParam, string name, SqlType sqlType)
		{
			base.InitializeParameter(dbParam, name, sqlType);
			if (sqlType.DbType == DbType.String || sqlType.DbType == DbType.StringFixedLength)
			{
				dbParam.DbType = DbType.Object;
				//((NpgsqlParameter)dbParam).ResetDbType(); not work - cast parameters to text and compare to citext failed (citext also casted to text)
			}
		}
	}
}
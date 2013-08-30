using System;
using System.Data;
using NHibernate.SqlTypes;

namespace AGO.Core.Nullables
{
	public class NullableDateTime : NullableType<DateTime>
	{
		public override void Set(IDbCommand cmd, object value, int index)
		{
			var modifiedValue = value;
			//Convert dates to UTC before writing them to database
			if (value is DateTime)
			{
				var dateTime = (DateTime)value;
				if (dateTime.Kind == DateTimeKind.Unspecified)
					dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
				modifiedValue = dateTime.ToUniversalTime();

			}
			((IDbDataParameter)cmd.Parameters[index]).Value = modifiedValue;
		}

		public override object Get(IDataReader rs, int index)
		{
			var value = rs[index];
			if (value != null)
			{
				//Force dates received from database as UTC
				if (value is DateTime)
				{
					var dateTime = (DateTime)value;
					if (dateTime.Kind == DateTimeKind.Unspecified)
						dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
					value = dateTime.ToUniversalTime();
				}
			}
			return value == null ? null : new DateTime?((DateTime)value);
		}

		public NullableDateTime()
			: base(new SqlType(DbType.DateTime))
		{
		}
	}
}

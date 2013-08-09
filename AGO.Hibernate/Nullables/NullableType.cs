using System;
using System.Data;
using NHibernate.SqlTypes;
using NHibernate.Type;

namespace AGO.Hibernate.Nullables
{
	public abstract class NullableType<TClrType> : ImmutableType where TClrType : struct
	{
		protected NullableType(SqlType sqlType)
			: base(sqlType)
		{
		}

		public override string Name
		{
			get { return GetType().Name; }
		}

		public override Type ReturnedClass
		{
			get { return typeof(TClrType?); }
		}

		public override void Set(IDbCommand cmd, object value, int index)
		{
			((IDbDataParameter)cmd.Parameters[index]).Value = value;
		}

		public override object Get(IDataReader rs, int index)
		{
			return (TClrType)rs[index];
		}

		public override object Get(IDataReader rs, string name)
		{
			var index = rs.GetOrdinal(name);
			return Get(rs, index);
		}

		public override string ToString(object val)
		{
			return val == null ? "" : val.ToString();
		}

		public override object FromStringValue(string xml)
		{
			return null;
		}
	}
}

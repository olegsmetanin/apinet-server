using System;
using System.Data;
using System.IO;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace AGO.Core.Model.Lob
{
	public class ClobType : AbstractLobType
	{
		protected override object Get(IDataReader rs, int ordinal)
		{
			return new StringClob(rs.GetString(ordinal));
		}

		protected override object GetData(object value)
		{
			var clob = value as Clob;
			if (clob == null)
				return null;			
			if (clob.Equals(Clob.Empty)) 
				return "";
			var sc = clob as StringClob;
			if (sc != null) 
				return sc.Text;
			using (var sw = new StringWriter())
			{
				clob.WriteTo(sw);
				return sw.ToString();
			}
		}

		protected override object GetValue(object dataObj)
		{
			var str = dataObj as string;
			return str == null ? null : new StringClob(str);
		}

		public override SqlType[] SqlTypes(IMapping mapping)
		{
			return new SqlType[] { new StringClobSqlType() };		
		}

		public override Type ReturnedClass
		{
			get { return typeof(Clob); }
		}
	}
}
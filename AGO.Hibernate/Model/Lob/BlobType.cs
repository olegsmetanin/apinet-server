using System;
using System.IO;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace AGO.Hibernate.Model.Lob
{
	public class BlobType : AbstractLobType
	{
		protected override object GetData(object value)
		{
			var blob = value as Blob;
			if (blob == null) 
				return null;
			var ab = blob as ArrayBlob;
			if (ab != null) 
				return ab.Data;
			using (var data = new MemoryStream())
			{
				blob.WriteTo(data);
				return data.ToArray();
			}
		}

		protected override object GetValue(object dataObj)
		{
			var data = dataObj as byte[];
			return data == null ? null : new ArrayBlob(data);
		}

		public override SqlType[] SqlTypes(IMapping mapping)
		{
			return new SqlType[] { new BinaryBlobSqlType() };
		}

		public override Type ReturnedClass
		{
			get { return typeof(Blob); }
		}
	}
}
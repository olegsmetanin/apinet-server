using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Xml;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Type;

namespace AGO.Core.Model.Lob
{
	public abstract class AbstractLobType : AbstractType
	{
		protected abstract object GetData(object value);

		protected abstract object GetValue(object data);

		protected virtual object Get(IDataReader rs, int ordinal)
		{
			const int bufferSize = 0x1000;
			var buffer = new byte[bufferSize];

			var readBytes = (int)rs.GetBytes(ordinal, 0L, buffer, 0, bufferSize);
			long position = readBytes;
			using (var data = new MemoryStream(readBytes))
			{
				if (readBytes >= bufferSize)
					while (readBytes > 0)
					{
						data.Write(buffer, 0, readBytes);
						position += (readBytes = (int)rs.GetBytes(ordinal, position, buffer, 0, bufferSize));
					}

				data.Write(buffer, 0, readBytes);
				data.Flush();
				return GetValue(data.Length == 0 ? new byte[0] : data.ToArray());
			}
		}

		public override string ToLoggableString(object value, ISessionFactoryImplementor factory)
		{
			return "[LOB]";
		}

		public override object DeepCopy(object value, EntityMode entityMode, ISessionFactoryImplementor factory)
		{
			return value;
		}

		public override object Replace(object original, object target, ISessionImplementor session, object owner, IDictionary copiedAlready)
		{
			return original;
		}

		public override void SetToXMLNode(XmlNode node, object value, ISessionFactoryImplementor factory)
		{
			node.Value = null;
		}

		public override object Assemble(object cached, ISessionImplementor session, object owner)
		{
			return GetValue(cached);
		}

		public override object Disassemble(object value, ISessionImplementor session, object owner)
		{
			return GetData(value);
		}

		public override void NullSafeSet(IDbCommand cmd, object value, int index, bool[] settable, ISessionImplementor session)
		{
			if (settable[0]) NullSafeSet(cmd, value, index, session);
		}

		public override void NullSafeSet(IDbCommand cmd, object value, int index, ISessionImplementor session)
		{
			var data = GetData(value);
			((IDataParameter)cmd.Parameters[index]).Value = data ?? DBNull.Value;
		}

		public override object NullSafeGet(IDataReader rs, string[] names, ISessionImplementor session, object owner)
		{
			return NullSafeGet(rs, names[0], session, owner);
		}

		public override object NullSafeGet(IDataReader rs, string name, ISessionImplementor session, object owner)
		{
			var i = rs.GetOrdinal(name);
			return rs.IsDBNull(i) ? null : Get(rs, i);
		}

		public override int GetColumnSpan(IMapping session)
		{
			return 1;
		}

		public override bool IsDirty(object old, object current, bool[] checkable, ISessionImplementor session)
		{
			return checkable[0] && IsDirty(old, current, session);
		}

		public override object FromXMLNode(XmlNode xml, IMapping factory)
		{
			return null;
		}

		public override bool[] ToColumnNullness(object value, IMapping mapping)
		{
			return value == null ? new[] { false } : new[] { true };
		}

		public override bool IsMutable
		{
			get { return true; }
		}

		public override bool Equals(object obj)
		{
			if (this == obj) 
				return true;
			if (obj == null) 
				return false;
			return GetType() == obj.GetType();
		}

		public override int GetHashCode()
		{
			return GetType().GetHashCode();
		}

		public override string Name
		{
			get { return GetType().Name; }
		}
	}
}

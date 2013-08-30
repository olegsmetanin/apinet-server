using System.IO;
using System.Text;

namespace AGO.Core.Model.Lob
{
	public class EmptyClob : Clob
	{
		public override TextReader OpenReader()
		{
			return new StringReader("");
		}

		public override void WriteTo(TextWriter writer)
		{
		}

		public override void WriteTo(Stream output, Encoding encoding)
		{
		}

		public override bool Equals(Clob clob)
		{
			var ec = clob as EmptyClob;
			if (ec != null) return true;
			var sc = clob as StringClob;
			return sc != null && sc.Text == "";
		}
	}
}
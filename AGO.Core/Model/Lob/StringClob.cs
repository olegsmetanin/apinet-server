using System;
using System.IO;

namespace AGO.Core.Model.Lob
{
	public class StringClob : Clob
	{
		private readonly string _Text;

		public string Text
		{
			get
			{
				return _Text;
			}
		}

		public StringClob(string text)
		{
			if (text == null) 
				throw new ArgumentNullException("text");
			_Text = text;
		}

		public override TextReader OpenReader()
		{
			return new StringReader(_Text);
		}

		public override void WriteTo(TextWriter writer)
		{
			writer.Write(_Text);
		}

		public override int GetHashCode()
		{
			return _Text!=null ? _Text.GetHashCode() : base.GetHashCode();
		}

		public override bool Equals(Clob clob)
		{
			if (clob == this) 
				return true;
			var sc = clob as StringClob;
			return sc != null && _Text.Equals(sc._Text);
		}
	}
}
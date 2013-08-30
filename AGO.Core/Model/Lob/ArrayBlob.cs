using System;
using System.IO;
using System.Linq;

namespace AGO.Core.Model.Lob
{
	public class ArrayBlob : Blob
	{
		private readonly byte[] _Data;

		public byte[] Data
		{
			get
			{
				return _Data;
			}
		}

		public ArrayBlob(byte[] data)
		{
			if (data == null) throw new ArgumentNullException("data");
			_Data = data;
		}

		public override Stream OpenReader()
		{
			return new MemoryStream(_Data, false);
		}

		public override void WriteTo(Stream output)
		{
			output.Write(_Data, 0, _Data.Length);
		}

		public override int GetHashCode()
		{
			return _Data != null ? _Data.GetHashCode() : base.GetHashCode();
		}

		public override bool Equals(Blob blob)
		{
			var ab = blob as ArrayBlob;
			if (ab == null) 
				return false;
			if (this == ab)
				return true;

			byte[] a = _Data, b = ab._Data;

			if (a.Length != b.Length) 
				return false;

			return !a.Where((t, i) => t != b[i]).Any();
		}
	}
}
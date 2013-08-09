using System.IO;

namespace AGO.Hibernate.Model.Lob
{
	public abstract class Blob
	{
		private const int Buffersize = 0x1000;

		public static Blob Empty
		{
			get
			{
				return new EmptyBlob();
			}
		}

		public static Blob Create(Stream input)
		{
			return new StreamBlob(input);
		}

		public static Blob Create(byte[] data)
		{
			return new ArrayBlob(data);
		}

		public static implicit operator Blob(Stream input)
		{
			return new StreamBlob(input);
		}

		public static implicit operator Blob(byte[] data)
		{
			return new ArrayBlob(data);
		}

		public abstract Stream OpenReader();

		public virtual void WriteTo(Stream output)
		{
			using (var s = OpenReader())
			{
				var buffer = new byte[Buffersize];
				int readBytes;
				while ((readBytes = s.Read(buffer, 0, Buffersize)) > 0)
				{
					output.Write(buffer, 0, readBytes);
				}
			}
		}

		public override bool Equals(object obj)
		{
			var b = obj as Blob;
			return b != null && Equals(b);
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public abstract bool Equals(Blob blob);
	}
}
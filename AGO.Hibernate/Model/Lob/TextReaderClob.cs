using System;
using System.IO;
using System.Text;

namespace AGO.Hibernate.Model.Lob
{
	public class TextReaderClob : Clob
	{
		private readonly TextReader _Reader;
		private bool _NeedRestart;
		private readonly long _InitialPosition;
		private bool _AlreadyOpen;

		public TextReaderClob(Stream stream, Encoding encoding)
		{
			if (stream == null) throw new ArgumentNullException("stream");
			if (encoding == null) throw new ArgumentNullException("encoding");
			try
			{
				_InitialPosition = stream.CanSeek ? stream.Position : -1L;
			}
			catch
			{
				_InitialPosition = -1L;
			}
			_Reader = new StreamReader(stream, encoding);
		}

		public TextReaderClob(TextReader reader)
		{
			if (reader == null) 
				throw new ArgumentNullException("reader");
			var sr = reader as StreamReader;
			if (sr != null)
			{
				try
				{
					_InitialPosition = sr.BaseStream.CanSeek ? sr.BaseStream.Position : -1L;
				}
				catch
				{
					_InitialPosition = -1L;
				}
			}
			else
				_InitialPosition = -1L;
			_Reader = reader;
		}

		public override TextReader OpenReader()
		{
			lock (this)
			{
				var streamReader = _Reader as StreamReader;
				if (_NeedRestart && _InitialPosition < 0L)
					throw new Exception("The underlying TextReader cannot be reset. It has already been opened.");
				if (_AlreadyOpen)
					throw new Exception("There's already a reader open on this Clob. Close the first reader before requesting a new one.");
				if (_NeedRestart && streamReader != null)
					streamReader.BaseStream.Seek(_InitialPosition, SeekOrigin.Begin);
				_AlreadyOpen = true;
			}
			return new ClobReader(this);
		}

		public override bool Equals(Clob clob)
		{
			var rc = clob as TextReaderClob;
			if (rc == null) return false;
			if (rc == this)
				return true;
			return _Reader == rc._Reader;
		}

		public override int GetHashCode()
		{
			return DateTime.UtcNow.GetHashCode();
		}

		#region Read-only stream wrapper class

		private class ClobReader : TextReader
		{
			private TextReaderClob _Clob;

			public ClobReader(TextReaderClob clob)
			{
				_Clob = clob;
			}

			private void ThrowClosed()
			{
				if (_Clob == null) throw new Exception("The TextReader is already closed.");
			}

			public override void Close()
			{
				Dispose(true);
			}

			protected override void Dispose(bool disposing)
			{
				if (_Clob != null)
					lock (_Clob)
					{
						_Clob._AlreadyOpen = false;
						_Clob = null;
					}
			}

			public override int Peek()
			{
				ThrowClosed();
				return _Clob._Reader.Peek();
			}

			public override int Read()
			{
				ThrowClosed();
				_Clob._NeedRestart = true;
				return _Clob._Reader.Read();
			}

			public override int Read(char[] buffer, int index, int count)
			{
				ThrowClosed();
				if (index > 0 || count > 0) _Clob._NeedRestart = true;
				return _Clob._Reader.Read(buffer, index, count);
			}

			public override int ReadBlock(char[] buffer, int index, int count)
			{
				ThrowClosed();
				if (index > 0 || count > 0) _Clob._NeedRestart = true;
				return _Clob._Reader.ReadBlock(buffer, index, count);
			}

			public override string ReadLine()
			{
				ThrowClosed();
				_Clob._NeedRestart = true;
				return _Clob._Reader.ReadLine();
			}

			public override string ReadToEnd()
			{
				ThrowClosed();
				_Clob._NeedRestart = true;
				return _Clob._Reader.ReadToEnd();
			}
		}

		#endregion
	}
}
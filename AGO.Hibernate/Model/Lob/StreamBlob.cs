using System;
using System.IO;

namespace AGO.Hibernate.Model.Lob
{
	public class StreamBlob : Blob
	{
		private readonly Stream _Stream;
		private readonly long _InitialPosition;
		private bool _NeedRestart;
		private bool _AlreadyOpen;

		public Stream UnderlyingStream
		{
			get
			{
				return _Stream;
			}
		}

		public StreamBlob(Stream stream)
		{
			if (stream == null) 
				throw new ArgumentNullException("stream");
			if (!stream.CanRead) 
				throw new NotSupportedException("Stream cannot read. Blobs are read-only.");

			_Stream = stream;
			try
			{
				_InitialPosition = stream.CanSeek ? stream.Position : -1L;
			}
			catch
			{
				_InitialPosition = -1L;
			}
		}

		public override Stream OpenReader()
		{
			lock (this)
			{
				if (_NeedRestart && _InitialPosition < 0L)
					throw new Exception("The underlying Stream cannot be reset. It has already been opened.");
				if (_AlreadyOpen)
					throw new Exception("There's already a reader Stream open on this Blob. Close the first Stream before requesting a new one.");
				if (_NeedRestart)
					_Stream.Seek(_InitialPosition, SeekOrigin.Begin);
				_AlreadyOpen = true;
			}
			return new BlobStream(this);
		}

		public override int GetHashCode()
		{
			return DateTime.UtcNow.GetHashCode();
		}

		public override bool Equals(Blob blob)
		{
			var sb = blob as StreamBlob;
			if (sb == null)
				return false;
			if (_Stream == sb._Stream)
				return true;
			var fsa = _Stream as FileStream;
			if (fsa == null)
				return false;
			var fsb = sb._Stream as FileStream;
			if (fsb == null)
				return false;
			try
			{
				return fsa.Name.Equals(fsb.Name);
			}
			catch
			{
				return false;
			}
		}

		#region Read-only stream wrapper class

		private class BlobStream : Stream
		{
			private StreamBlob _Blob;

			public BlobStream(StreamBlob blob)
			{
				_Blob = blob;
			}

			private void ThrowClosed()
			{
				if (_Blob == null)
					throw new Exception("The Stream is already closed.");
			}

			public override bool CanRead
			{
				get { return _Blob != null && _Blob._Stream.CanRead; }
			}

			public override bool CanSeek
			{
				get
				{
					return _Blob != null && _Blob._Stream.CanSeek;
				}
			}

			public override bool CanWrite
			{
				get { return false; }
			}

			public override void Flush()
			{
				throw new NotSupportedException();
			}

			public override long Length
			{
				get
				{
					ThrowClosed();
					return _Blob._Stream.Length;
				}
			}

			public override long Position
			{
				get
				{
					ThrowClosed();
					return _Blob._Stream.Position;
				}
				set
				{
					ThrowClosed();
					_Blob._Stream.Position = value;
					_Blob._NeedRestart = true;
				}
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				ThrowClosed();
				var i = _Blob._Stream.Read(buffer, offset, count);
				if (i > 0) _Blob._NeedRestart = true;
				return i;
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				ThrowClosed();
				_Blob._NeedRestart = true;
				return _Blob._Stream.Seek(offset, origin);
			}

			public override void SetLength(long value)
			{
				throw new NotSupportedException();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				throw new NotSupportedException();
			}

			public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
			{
				ThrowClosed();
				_Blob._NeedRestart = true;
				return _Blob._Stream.BeginRead(buffer, offset, count, callback, state);
			}

			public override int EndRead(IAsyncResult asyncResult)
			{
				ThrowClosed();
				return _Blob._Stream.EndRead(asyncResult);
			}

			public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
			{
				throw new NotSupportedException();
			}

			public override void EndWrite(IAsyncResult asyncResult)
			{
				throw new NotSupportedException();
			}

			public override bool CanTimeout
			{
				get
				{
					return _Blob != null && _Blob._Stream.CanTimeout;
				}
			}

			public override void Close()
			{
				Dispose(true);
			}

			protected override void Dispose(bool disposing)
			{
				if (_Blob != null)
					lock (_Blob)
					{
						_Blob._AlreadyOpen = false;
						_Blob = null;
					}
			}

			public override int ReadByte()
			{
				ThrowClosed();
				var i = _Blob._Stream.ReadByte();
				_Blob._NeedRestart = true;
				return i;
			}

			public override void WriteByte(byte value)
			{
				throw new NotSupportedException();
			}

			public override int ReadTimeout
			{
				get
				{
					ThrowClosed();
					return _Blob._Stream.ReadTimeout;
				}
				set
				{
					ThrowClosed();
					_Blob._Stream.ReadTimeout = value;
				}
			}

			public override int WriteTimeout
			{
				get
				{
					throw new NotSupportedException();
				}
				set
				{
					throw new NotSupportedException();
				}
			}
		}

		#endregion
	}
}
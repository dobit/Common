using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.IO;

namespace System.Threading
{
    /// <summary>Writeable stream for using a separate thread in a producer/consumer scenario.</summary>
    public sealed class TransferStream : AbstractStreamBase
    {
        /// <summary>The collection of chunks to be written.</summary>
        private BlockingCollection<byte[]> _chunks;
        /// <summary>The Task to use for background writing.</summary>
        private Task _processingTask;
        /// <summary>The underlying stream to target.</summary>
        private Stream _writeableStream;

        /// <summary>Initializes a new instance of the TransferStream.</summary>
        /// <param name="writeableStream">The underlying stream to which to write.</param>
        public TransferStream(Stream writeableStream)
        {
            Action action = null;
            if (writeableStream == null)
            {
                throw new ArgumentNullException("writeableStream");
            }
            if (!writeableStream.CanWrite)
            {
                throw new ArgumentException("Target stream is not writeable.");
            }
            this._writeableStream = writeableStream;
            this._chunks = new BlockingCollection<byte[]>();
            if (action == null)
            {
                action = delegate {
                    foreach (byte[] buffer in this._chunks.GetConsumingEnumerable())
                    {
                        this._writeableStream.Write(buffer, 0, buffer.Length);
                    }
                };
            }
            this._processingTask = Task.Factory.StartNew(action, TaskCreationOptions.LongRunning);
        }

        /// <summary>Closes the stream and releases all resources associated with it.</summary>
        public override void Close()
        {
            this._chunks.CompleteAdding();
            try
            {
                this._processingTask.Wait();
            }
            finally
            {
                base.Close();
            }
        }

        /// <summary>Writes a sequence of bytes to the stream.</summary>
        /// <param name="buffer">An array of bytes. Write copies count bytes from buffer to the stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset >= buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || ((offset + count) > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (count != 0)
            {
                byte[] dst = new byte[count];
                Buffer.BlockCopy(buffer, offset, dst, 0, count);
                this._chunks.Add(dst);
            }
        }

        /// <summary>Determines whether data can be written to the stream.</summary>
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }
    }
}


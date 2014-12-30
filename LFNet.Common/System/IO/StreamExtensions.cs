using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace System.IO
{
    /// <summary>Extension methods for asynchronously working with streams.</summary>
    public static class StreamExtensions
    {
        private const int BUFFER_SIZE = 0x2000;

        /// <summary>
        /// Creates an enumerable to be used with TaskFactoryExtensions.Iterate that copies data from one
        /// stream to another.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="output">The output stream.</param>
        /// <returns>An enumerable containing yielded tasks from the copy operation.</returns>
        private static IEnumerable<Task> CopyStreamIterator(Stream input, Stream output)
        {
            byte[][] iteratorVariable0 = new byte[][] { new byte[BUFFER_SIZE], new byte[BUFFER_SIZE] };
            int index = 0;
            Task iteratorVariable2 = null;
            while (true)
            {
                Task<int> iteratorVariable3 = input.ReadAsync(iteratorVariable0[index], 0, iteratorVariable0[index].Length);
                if (iteratorVariable2 == null)
                {
                    yield return iteratorVariable3;
                    iteratorVariable3.Wait();
                }
                else
                {
                    Task[] tasks = new Task[] { iteratorVariable3, iteratorVariable2 };
                    yield return Task.Factory.WhenAll(tasks);
                    Task.WaitAll(tasks);
                }
                if (iteratorVariable3.Result <= 0)
                {
                    yield break;
                }
                iteratorVariable2 = output.WriteAsync(iteratorVariable0[index], 0, iteratorVariable3.Result);
                index ^= 1;
            }
        }

        /// <summary>Copies the contents of a stream to a file, asynchronously.</summary>
        /// <param name="source">The source stream.</param>
        /// <param name="destinationPath">The path to the destination file.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        public static Task CopyStreamToFileAsync(this Stream source, string destinationPath)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (destinationPath == null)
            {
                throw new ArgumentNullException("destinationPath");
            }
            FileStream destinationStream = FileAsync.OpenWrite(destinationPath);
            return source.CopyStreamToStreamAsync(destinationStream).ContinueWith(delegate (Task t) {
                AggregateException exception = t.Exception;
                destinationStream.Close();
                if (exception != null)
                {
                    throw exception;
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>Copies the contents of one stream to another, asynchronously.</summary>
        /// <param name="source">The source stream.</param>
        /// <param name="destination">The destination stream.</param>
        /// <returns>A Task that represents the completion of the asynchronous operation.</returns>
        public static Task CopyStreamToStreamAsync(this Stream source, Stream destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            return Task.Factory.Iterate(CopyStreamIterator(source, destination));
        }

        /// <summary>Reads the contents of the stream asynchronously.</summary>
        /// <param name="stream">The stream.</param>
        /// <returns>A Task representing the contents of the file in bytes.</returns>
        public static Task<byte[]> ReadAllBytesAsync(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            int capacity = stream.CanSeek ? ((int) stream.Length) : 0;
            MemoryStream readData = new MemoryStream(capacity);
            return stream.CopyStreamToStreamAsync(readData).ContinueWith<byte[]>(delegate (Task t) {
                t.PropagateExceptions();
                return readData.ToArray();
            });
        }

        /// <summary>Read from a stream asynchronously.</summary>
        /// <param name="stream">The stream.</param>
        /// <param name="buffer">An array of bytes to be filled by the read operation.</param>
        /// <param name="offset">The offset at which data should be stored.</param>
        /// <param name="count">The number of bytes to be read.</param>
        /// <returns>A Task containing the number of bytes read.</returns>
        public static Task<int> ReadAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            return Task<int>.Factory.FromAsync<byte[], int, int>(new Func<byte[], int, int, AsyncCallback, object, IAsyncResult>(stream.BeginRead), new Func<IAsyncResult, int>(stream.EndRead), buffer, offset, count, stream);
        }

        /// <summary>Read the content of the stream, yielding its data in buffers to the provided delegate.</summary>
        /// <param name="stream">The stream.</param>
        /// <param name="bufferSize">The size of the buffers to use.</param>
        /// <param name="bufferAvailable">The delegate to be called when a new buffer is available.</param>
        /// <returns>A Task that represents the completion of the asynchronous operation.</returns>
        public static Task ReadBuffersAsync(this Stream stream, int bufferSize, Action<byte[], int> bufferAvailable)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (bufferSize < 1)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }
            if (bufferAvailable == null)
            {
                throw new ArgumentNullException("bufferAvailable");
            }
            return Task.Factory.Iterate(ReadIterator(stream, bufferSize, bufferAvailable));
        }

        /// <summary>
        /// Creates an enumerable to be used with TaskFactoryExtensions.Iterate that reads data
        /// from an input stream and passes it to a user-provided delegate.
        /// </summary>
        /// <param name="input">The source stream.</param>
        /// <param name="bufferSize">The size of the buffers to be used.</param>
        /// <param name="bufferAvailable">
        /// A delegate to be invoked when a buffer is available (provided the
        /// buffer and the number of bytes in the buffer starting at offset 0.
        /// </param>
        /// <returns>An enumerable containing yielded tasks from the operation.</returns>
        private static IEnumerable<Task> ReadIterator(Stream input, int bufferSize, Action<byte[], int> bufferAvailable)
        {
            byte[] buffer = new byte[bufferSize];
            while (true)
            {
                Task<int> iteratorVariable1 = input.ReadAsync(buffer, 0, buffer.Length);
                yield return iteratorVariable1;
                if (iteratorVariable1.Result <= 0)
                {
                    yield break;
                }
                bufferAvailable(buffer, iteratorVariable1.Result);
            }
        }

        /// <summary>Write to a stream asynchronously.</summary>
        /// <param name="stream">The stream.</param>
        /// <param name="buffer">An array of bytes to be written.</param>
        /// <param name="offset">The offset from which data should be read to be written.</param>
        /// <param name="count">The number of bytes to be written.</param>
        /// <returns>A Task representing the completion of the asynchronous operation.</returns>
        public static Task WriteAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            return Task.Factory.FromAsync<byte[], int, int>(new Func<byte[], int, int, AsyncCallback, object, IAsyncResult>(stream.BeginWrite), new Action<IAsyncResult>(stream.EndWrite), buffer, offset, count, stream);
        }


    }
}


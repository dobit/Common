using System;
using System.IO;
using System.Net;

namespace LFNet.Common.Net
{
    public class HttpFileStream:Stream
    {
        private static object locker=new object();

        private readonly Stream _soureceStream;
        private readonly FileInfo _fileInfo;
        private readonly FileStream _fileStream;
        private bool _finished;
        private readonly string _tempFile;
        private readonly DateTime _lastModifiedTime;

        internal HttpFileStream(HttpWebResponse response, FileInfo fileInfo)
        {

            _lastModifiedTime = response.LastModified;
            _fileInfo = fileInfo; // 目标文件
            _tempFile = fileInfo.FullName + "_lf";
            _soureceStream = response.GetResponseStream();
            try
            {
                lock (locker)
                {
                    _fileStream = new FileStream(fileInfo.FullName + "_lf", FileMode.Create, FileAccess.Write,
                                             FileShare.None);
                }
            }
            catch
            {

                _fileStream = null;
            }
        }

        #region Overrides of Stream

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception><filterpriority>2</filterpriority>
        public override void Flush()
        {
            _soureceStream.Flush();
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <param name="offset">A byte offset relative to the origin parameter. </param><param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position. </param><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _soureceStream.Seek(offset, origin);
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes. </param><exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>2</filterpriority>
        public override void SetLength(long value)
        {
            _soureceStream.SetLength(value);
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream. </param><param name="count">The maximum number of bytes to be read from the current stream. </param><param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source. </param><exception cref="T:System.ArgumentException">The sum of offset and count is larger than the buffer length. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><exception cref="T:System.NotSupportedException">The stream does not support reading. </exception><exception cref="T:System.ArgumentNullException">buffer is null. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception><filterpriority>1</filterpriority>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int ret=_soureceStream.Read(buffer, offset, count);
            
                if (ret > 0)
                {
                    if (_fileStream!= null)
                    _fileStream.Write(buffer, offset, ret);
                }
                else
                {
                    if (_fileStream != null)
                    {
                        
                            _fileStream.Flush();
                        lock (locker)
                        {
                            _fileStream.Dispose();
                            try
                            {
                                _fileInfo.Delete();
                                //File.Delete(_fileInfo.FullName);
                                File.Move(_tempFile, _fileInfo.FullName);
                                if (_lastModifiedTime != DateTime.MinValue)
                                    _fileInfo.LastWriteTime = _lastModifiedTime;

                            }
                            catch
                            {

                                File.Delete(_tempFile);

                            }
                        }
                        _finished = true;
                    }
                }
          
            return ret;
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream. </param><param name="count">The number of bytes to be written to the current stream. </param><param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream. </param><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support writing. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><exception cref="T:System.ArgumentNullException">buffer is null. </exception><exception cref="T:System.ArgumentException">The sum of offset and count is greater than the buffer length. </exception><exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception><filterpriority>1</filterpriority>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _soureceStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>
        /// true if the stream supports reading; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanRead
        {
            get { return _soureceStream.CanRead; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>
        /// true if the stream supports seeking; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanSeek
        {
            get { return _soureceStream.CanSeek; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>
        /// true if the stream supports writing; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanWrite
        {
            get { return _soureceStream.CanWrite; }
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <returns>
        /// A long value representing the length of the stream in bytes.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
        public override long Length
        {
            get { return _soureceStream.Length; }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The current position within the stream.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
        public override long Position
        {
            get { return _soureceStream.Position; }
            set { _soureceStream.Position = value; }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (_fileStream != null && !_finished)
            {
                lock (locker)
                {
                    _fileStream.Dispose();
                    File.Delete(_tempFile);
                }
            }
            _soureceStream.Dispose();
            base.Dispose(disposing);
            
        }


    }
}
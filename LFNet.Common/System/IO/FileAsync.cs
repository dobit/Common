using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    /// <summary>Provides asynchronous counterparts to members of the File class.</summary>
    public static class FileAsync
    {
        private const int BUFFER_SIZE = 0x2000;

        /// <summary>Opens an existing file for asynchronous reading.</summary>
        /// <param name="path">The path to the file to be opened for reading.</param>
        /// <returns>A read-only FileStream on the specified path.</returns>
        public static FileStream OpenRead(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, true);
        }

        /// <summary>Opens an existing file for asynchronous writing.</summary>
        /// <param name="path">The path to the file to be opened for writing.</param>
        /// <returns>An unshared FileStream on the specified path with access for writing.</returns>
        public static FileStream OpenWrite(string path)
        {
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, BUFFER_SIZE, true);
        }

        /// <summary>
        /// Opens a binary file for asynchronosu operation, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">The path to the file to be read.</param>
        /// <returns>A task that will contain the contents of the file.</returns>
        public static Task<byte[]> ReadAllBytes(string path)
        {
            FileStream fs = OpenRead(path);
            return fs.ReadAllBytesAsync().ContinueWith<byte[]>(delegate (Task<byte[]> t) {
                fs.Close();
                return t.Result;
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Opens a text file for asynchronosu operation, reads the contents of the file into a string, and then closes the file.
        /// </summary>
        /// <param name="path">The path to the file to be read.</param>
        /// <returns>A task that will contain the contents of the file.</returns>
        public static Task<string> ReadAllText(string path)
        {
            StringBuilder text = new StringBuilder();
            UTF8Encoding encoding = new UTF8Encoding();
            FileStream fs = OpenRead(path);
            return fs.ReadBuffersAsync(BUFFER_SIZE, delegate(byte[] buffer, int count)
            {
                text.Append(encoding.GetString(buffer, 0, count));
            }).ContinueWith<string>(delegate (Task t) {
                AggregateException exception = t.Exception;
                fs.Close();
                if (exception != null)
                {
                    throw exception;
                }
                return text.ToString();
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Opens a binary file for asynchronous operation, writes the contents of the byte array into the file, and then closes the file.
        /// </summary>
        /// <param name="path">The path to the file to be written.</param>
        /// <param name="bytes"></param>
        /// <returns>A task that will signal the completion of the operation.</returns>
        public static Task WriteAllBytes(string path, byte[] bytes)
        {
            FileStream fs = OpenWrite(path);
            return fs.WriteAsync(bytes, 0, bytes.Length).ContinueWith(delegate (Task t) {
                AggregateException exception = t.Exception;
                fs.Close();
                if (exception != null)
                {
                    throw exception;
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Opens a text file for asynchronous operation, writes a string into the file, and then closes the file.
        /// </summary>
        /// <param name="path">The path to the file to be written.</param>
        /// <param name="contents"></param>
        /// <returns>A task that will signal the completion of the operation.</returns>
        public static Task WriteAllText(string path, string contents)
        {
            return Task.Factory.StartNew<byte[]>(state => Encoding.UTF8.GetBytes((string) state), contents).ContinueWith<Task>(t => WriteAllBytes(path, t.Result)).Unwrap();
        }
    }
}


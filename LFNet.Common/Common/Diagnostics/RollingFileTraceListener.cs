using System;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;

namespace LFNet.Common.Diagnostics
{
    /// <summary>
    /// Writes tracing or debugging output to a text file.
    /// </summary>
    [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
    public class RollingFileTraceListener : TraceListener
    {
        private DateTime _currentDate;
        private readonly string _fileName;
        private StreamWriter _traceWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Diagnostics.RollingFileTraceListener" /> class.
        /// </summary>
        public RollingFileTraceListener() : this(GetDefaultPath(), "RollingFileTraceListener")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Diagnostics.RollingFileTraceListener" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public RollingFileTraceListener(string fileName) : this(fileName, "RollingFileTraceListener")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Diagnostics.RollingFileTraceListener" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="name">The name.</param>
        public RollingFileTraceListener(string fileName, string name) : base(name)
        {
            this._fileName = fileName;
            this._traceWriter = new StreamWriter(this.GenerateFilename(), true);
        }

        private void CheckRollover()
        {
            if (this._traceWriter == null)
            {
                this._traceWriter = new StreamWriter(this.GenerateFilename(), true);
            }
            if (this._currentDate.CompareTo(DateTime.Today) != 0)
            {
                this.Close();
                this._traceWriter = new StreamWriter(this.GenerateFilename(), true);
            }
        }

        /// <summary>
        /// When overridden in a derived class, closes the output stream so it no longer receives tracing or debugging output.
        /// </summary>
        public override void Close()
        {
            if (this._traceWriter != null)
            {
                this._traceWriter.Close();
                this._traceWriter = null;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Diagnostics.TraceListener" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
            }
        }

        /// <summary>
        /// When overridden in a derived class, flushes the output buffer.
        /// </summary>
        public override void Flush()
        {
            if (this._traceWriter != null)
            {
                this._traceWriter.Flush();
            }
        }

        private string GenerateFilename()
        {
            this._currentDate = DateTime.Today;
            string directoryName = Path.GetDirectoryName(this._fileName);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this._fileName);
            string extension = Path.GetExtension(this._fileName);
            string str4 = string.Format("{0}_{1}{2}", fileNameWithoutExtension, this._currentDate.ToString("yyyymmdd"), extension);
            return Path.Combine(directoryName, str4);
        }

        private static string GetDefaultPath()
        {
            object data = AppDomain.CurrentDomain.GetData("DataDirectory");
            string str = (data == null) ? string.Empty : data.ToString();
            str = Path.Combine(str, "Logs");
            if (!Directory.Exists(str))
            {
                Directory.CreateDirectory(str);
            }
            return Path.GetFullPath(Path.Combine(str, "log.txt"));
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void Write(string value)
        {
            this.CheckRollover();
            this._traceWriter.Write(value);
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void WriteLine(string value)
        {
            this.CheckRollover();
            this._traceWriter.WriteLine(value);
        }
    }
}


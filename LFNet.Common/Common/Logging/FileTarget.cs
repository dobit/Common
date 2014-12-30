namespace LFNet.Common.Logging
{
    public class FileTarget : LogTarget, System.IDisposable
    {
        private readonly string _fileName;
        private readonly string _filePath;
        private System.IO.FileStream _fileStream;
        private System.IO.StreamWriter _logStream;
        private bool _disposed = false;
        public FileTarget(string fileName, Logger.Level minLevel, Logger.Level maxLevel, bool includeTimeStamps, bool reset = false)
        {
            this._fileName = fileName;
            this._filePath = string.Format("{0}/{1}", LogConfig.Instance.LoggingRoot, this._fileName);
            base.MinimumLevel = minLevel;
            base.MaximumLevel = maxLevel;
            base.IncludeTimeStamps = includeTimeStamps;
            if (!System.IO.Directory.Exists(LogConfig.Instance.LoggingRoot))
            {
                System.IO.Directory.CreateDirectory(LogConfig.Instance.LoggingRoot);
            }
            this._fileStream = new System.IO.FileStream(this._filePath, reset ? System.IO.FileMode.Create : System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read);
            this._logStream = new System.IO.StreamWriter(this._fileStream)
                                  {
                                      AutoFlush = true
                                  };
        }
        public override void LogMessage(Logger.Level level, string logger, string message)
        {
            bool flag = false;
            try
            {
#if NET4
                System.Threading.Monitor.Enter(this, ref flag);
#else
                System.Threading.Monitor.Enter(this);
                flag = true;
#endif
                string text = base.IncludeTimeStamps ? ("[" + System.DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff") + "] ") : "";
                if (!this._disposed)
                {
                    this._logStream.WriteLine(string.Format("{0}[{1}] [{2}]: {3}", new object[]
                                                                                       {
                                                                                           text,
                                                                                           level.ToString().PadLeft(5),
                                                                                           logger,
                                                                                           message
                                                                                       }));
                }
            }
            finally
            {
                if (flag)
                {
                    System.Threading.Monitor.Exit(this);
                }
            }
        }
        public override void LogException(Logger.Level level, string logger, string message, System.Exception exception)
        {
            bool flag = false;
            try
            {
#if NET4
                System.Threading.Monitor.Enter(this, ref flag);
#else
                System.Threading.Monitor.Enter(this);
                flag = true;
#endif
                string text = base.IncludeTimeStamps ? ("[" + System.DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff") + "] ") : "";
                if (!this._disposed)
                {
                    this._logStream.WriteLine(string.Format("{0}[{1}] [{2}]: {3} - [Exception] {4}", new object[]
                                                                                                         {
                                                                                                             text,
                                                                                                             level.ToString().PadLeft(5),
                                                                                                             logger,
                                                                                                             message,
                                                                                                             exception
                                                                                                         }));
                }
            }
            finally
            {
                if (flag)
                {
                    System.Threading.Monitor.Exit(this);
                }
            }
        }
        public void Dispose()
        {
            this.Dispose(true);
            System.GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    this._logStream.Close();
                    this._logStream.Dispose();
                    this._fileStream.Close();
                    this._fileStream.Dispose();
                }
                this._logStream = null;
                this._fileStream = null;
                this._disposed = true;
            }
        }
        ~FileTarget()
        {
            this.Dispose(false);
        }
    }
}
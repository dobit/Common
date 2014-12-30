using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace LFNet.Common.Logs
{
    /// <summary>
    /// 用来记录日志的,记录后保存到文本
    /// 自动添加时间，不需要加时间
    /// 采用压入队列背景线程执行不影响执行速度
    /// </summary>
    public  class SimpleLogger
    {
        private  int _processEachNumber = 20;

        /// <summary>
        /// 每次处理的记录数,默认20
        /// </summary>
        public  int ProcessEachNumber
        {
            get { return _processEachNumber; }
            set { _processEachNumber = value; }
        }

        private  string _logFileExtendName = "config";
        private  System.IO.TextWriter _messageWriter = null;
        private  System.IO.TextWriter _exceptionWriter = null;
        private  object locker = new object();
        ///<summary>
        /// 日志文件扩展名
        ///</summary>
        public  string LogFileExtendName
        {
            get { return _logFileExtendName; }
            set { _logFileExtendName = value; }
        }

        private  Queue _queues = Queue.Synchronized(new Queue());
        private  bool _IsRuning = false;
        /// <summary>
        /// 记录日志，格式化的采用string.format转化
        /// </summary>
        /// <param name="format">格式化串</param>
        /// <param name="args">参数</param>
        public  void Log(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }

        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="exception">异常</param>
        public  void Log(Exception exception)
        {

            LogInfo logInfo = new LogInfo(Thread.CurrentThread.ManagedThreadId, exception);
            AddAndRun(logInfo);
        }
        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="message"></param>
        public  void Log(string message)
        {

            LogInfo logInfo = new LogInfo(Thread.CurrentThread.ManagedThreadId, message);
            AddAndRun(logInfo);
        }
        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="o"></param>
        public  void Log(object o)
        {

            LogInfo logInfo = new LogInfo(Thread.CurrentThread.ManagedThreadId, o);
            AddAndRun(logInfo);
        }

        private  void AddAndRun(LogInfo logInfo)
        {
            if (_queues.Count > int.MaxValue) Thread.Sleep(100);
            _queues.Enqueue(logInfo);
            if (!_IsRuning)
            {
                lock (locker) //锁定锁
                {
                    if (!_IsRuning)
                    {
                        _IsRuning = true;
                        Thread thread = new Thread(Run);
                        thread.IsBackground = true;
                        thread.Start();

                    }
                }
            }

        }

        private  void Run()
        {
            try
            {
                List<LogInfo> messages = new List<LogInfo>(ProcessEachNumber);
                List<LogInfo> exceptions = new List<LogInfo>(ProcessEachNumber);
                while (_queues.Count > 0)
                {
                    LogInfo logInfo = _queues.Dequeue() as LogInfo;
                    if (logInfo.Message is Exception)
                    {
                        exceptions.Add(logInfo);
                        if (exceptions.Count == ProcessEachNumber)
                        {
                            LogToExceptionFile(exceptions);
                            exceptions = new List<LogInfo>(ProcessEachNumber);
                        }
                    }
                    else
                    {
                        messages.Add(logInfo);
                        if (messages.Count == ProcessEachNumber)
                        {
                            LogToMessageFile(messages);
                            messages = new List<LogInfo>(ProcessEachNumber);
                        }
                    }
                }
                if (messages.Count > 0)
                    LogToMessageFile(messages);
                if (exceptions.Count > 0)
                    LogToExceptionFile(exceptions);
            }
            catch (Exception exception)
            {
                Log(exception);
            }
            finally
            {
                CloseFile();
            }
            lock (locker) //锁定锁
            {
                _IsRuning = false;
            }
        }

        private  void CloseFile()
        {
            if (_messageWriter != null)
            {
                _messageWriter.Close();
                _messageWriter.Dispose();
                _messageWriter = null;
            }
            if (_exceptionWriter != null)
            {

                _exceptionWriter.Close();
                _exceptionWriter.Dispose();
                _exceptionWriter = null;
            }
        }

        private  void LogToMessageFile(List<LogInfo> messages)
        {
            StringBuilder sb = new StringBuilder();
            foreach (LogInfo logInfo in messages)
            {
                sb.AppendFormat("{0}\t{1}\t{2}\t\r\n", logInfo.LogTime, logInfo.ThreadId, logInfo.Message);
            }
            if (_messageWriter == null)
            {
                string file =
                    FilePathUtil.GetMapPath("/log/" + DateTime.Now.ToString("yyyyMMdd") + "ms." +
                                            LogFileExtendName);
                FileInfo fileInfo = new FileInfo(file);
                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }
                _messageWriter = new System.IO.StreamWriter(file, true, Encoding.UTF8);
            }
            _messageWriter.Write(sb.ToString());
        }

        private  void LogToExceptionFile(List<LogInfo> exceptions)
        {
            StringBuilder sb = new StringBuilder();
            foreach (LogInfo logInfo in exceptions)
            {
                sb.AppendFormat("{0}\t{1}\t{2}\t\r\n{3}\r\n", logInfo.LogTime, logInfo.ThreadId, ((Exception)logInfo.Message).Message, ((Exception)logInfo.Message).StackTrace);
            }
            if (_exceptionWriter == null)
            {
                string file =
                    FilePathUtil.GetMapPath("/log/" + DateTime.Now.ToString("yyyyMMdd") + "ex." +
                                            LogFileExtendName);
                FileInfo fileInfo = new FileInfo(file);
                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }
                _exceptionWriter = new System.IO.StreamWriter(file, true, Encoding.UTF8);
            }
            _exceptionWriter.Write(sb.ToString());
        }

        private class LogInfo
        {
            internal LogInfo(int threadId, object message)
            {
                ThreadId = threadId;
                Message = message;
                LogTime = DateTime.Now;
            }

            /// <summary>
            /// 线程Id
            /// </summary>
            public int ThreadId { get; set; }
            /// <summary>
            /// 消息
            /// </summary>
            public object Message { get; set; }

            /// <summary>
            /// 日志时间
            /// </summary>
            public DateTime LogTime { get; set; }
        }
    }


    
}
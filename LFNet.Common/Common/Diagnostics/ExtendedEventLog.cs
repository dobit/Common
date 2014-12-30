using System;
using System.Diagnostics;

namespace LFNet.Common.Diagnostics
{
    public class ExtendedEventLog : EventLog
    {
        private const string DEFAULT_EVENT_SOURCE = "CodeSmith";
        private const string DEFAULT_LOG_NAME = "CodeSmith";

        public ExtendedEventLog()
        {
        }

        public ExtendedEventLog(string logName) : base(logName)
        {
        }

        public ExtendedEventLog(string logName, string machineName) : base(logName, machineName)
        {
        }

        public ExtendedEventLog(string logName, string machineName, string source) : base(logName, machineName, source)
        {
        }

        public void WriteException(Exception e)
        {
            base.WriteEntry(e.ToString(), EventLogEntryType.Error);
        }

        public static ExtendedEventLog Default
        {
            get
            {
                return Nested.Default;
            }
        }

        private class Nested
        {
            internal static readonly ExtendedEventLog Default = new ExtendedEventLog(DEFAULT_LOG_NAME, ".", DEFAULT_EVENT_SOURCE);
        }
    }
}


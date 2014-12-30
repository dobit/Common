using System;
namespace LFNet.Common.Logging
{
    public static class LogManager
    {
        internal static readonly System.Collections.Generic.List<LogTarget> Targets = new System.Collections.Generic.List<LogTarget>();
        internal static readonly System.Collections.Generic.Dictionary<string, Logger> Loggers = new System.Collections.Generic.Dictionary<string, Logger>();
        public static bool Enabled
        {
            get;
            set;
        }
        public static void AttachLogTarget(LogTarget target)
        {
            LogManager.Targets.Add(target);
        }
        public static Logger CreateLogger()
        {
            System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, false);
            string name = stackFrame.GetMethod().DeclaringType.Name;
            if (name == null)
            {
                throw new System.Exception("Error getting full name for declaring type.");
            }
            if (!LogManager.Loggers.ContainsKey(name))
            {
                LogManager.Loggers.Add(name, new Logger(name));
            }
            return LogManager.Loggers[name];
        }
        public static Logger CreateLogger(string name)
        {
            if (!LogManager.Loggers.ContainsKey(name))
            {
                LogManager.Loggers.Add(name, new Logger(name));
            }
            return LogManager.Loggers[name];
        }
    }
}
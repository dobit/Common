namespace LFNet.Common.Logging
{
    public class LogTarget
    {
        public Logger.Level MinimumLevel
        {
            get;
            protected set;
        }
        public Logger.Level MaximumLevel
        {
            get;
            protected set;
        }
        public bool IncludeTimeStamps
        {
            get;
            protected set;
        }
        public virtual void LogMessage(Logger.Level level, string logger, string message)
        {
            throw new System.NotSupportedException();
        }
        public virtual void LogException(Logger.Level level, string logger, string message, System.Exception exception)
        {
            throw new System.NotSupportedException();
        }
    }
}
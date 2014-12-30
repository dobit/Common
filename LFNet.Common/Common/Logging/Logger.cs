namespace LFNet.Common.Logging
{
    public class Logger
    {
        public enum Level
        {
            Dump,
            Trace,
            Debug,
            Info,
            Warn,
            Error,
            Fatal
        }
        public string Name
        {
            get;
            protected set;
        }
        public Logger(string name)
        {
            this.Name = name;
        }
        private void Log(Logger.Level level, string message, object[] args)
        {
            LogRouter.RouteMessage(level, this.Name, (args == null) ? message : string.Format(System.Globalization.CultureInfo.InvariantCulture, message, args));
        }
        private void LogException(Logger.Level level, string message, object[] args, System.Exception exception)
        {
            LogRouter.RouteException(level, this.Name, (args == null) ? message : string.Format(System.Globalization.CultureInfo.InvariantCulture, message, args), exception);
        }
        public void Trace(string message)
        {
            this.Log(Logger.Level.Trace, message, null);
        }
        public void Trace(string message, params object[] args)
        {
            this.Log(Logger.Level.Trace, message, args);
        }
        public void Debug(string message)
        {
            this.Log(Logger.Level.Debug, message, null);
        }
        public void Debug(string message, params object[] args)
        {
            this.Log(Logger.Level.Debug, message, args);
        }
        public void Info(string message)
        {
            this.Log(Logger.Level.Info, message, null);
        }
        public void Info(string message, params object[] args)
        {
            this.Log(Logger.Level.Info, message, args);
        }
        public void Warn(string message)
        {
            this.Log(Logger.Level.Warn, message, null);
        }
        public void Warn(string message, params object[] args)
        {
            this.Log(Logger.Level.Warn, message, args);
        }
        public void Error(string message)
        {
            this.Log(Logger.Level.Error, message, null);
        }
        public void Error(string message, params object[] args)
        {
            this.Log(Logger.Level.Error, message, args);
        }
        public void Fatal(string message)
        {
            this.Log(Logger.Level.Fatal, message, null);
        }
        public void Fatal(string message, params object[] args)
        {
            this.Log(Logger.Level.Fatal, message, args);
        }
        //public void LogIncoming(IMessage msg, Header header)
        //{
        //    this.Log(Logger.Level.Dump, this.ShortHeader(header) + "[I] " + msg.AsText(), null);
        //}
        //public void LogOutgoing(IMessage msg, Header header)
        //{
        //    this.Log(Logger.Level.Dump, this.ShortHeader(header) + "[O] " + msg.AsText(), null);
        //}
        //private System.Text.StringBuilder ShortHeader(Header header)
        //{
        //    System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder("service_id: " + header.ServiceId);
        //    stringBuilder.Append(header.HasMethodId ? (" method_id: " + header.MethodId.ToString()) : "");
        //    stringBuilder.Append(header.HasToken ? (" token: " + header.Token.ToString()) : "");
        //    stringBuilder.Append(header.HasObjectId ? (" object_id: " + header.ObjectId.ToString()) : "");
        //    stringBuilder.Append(header.HasSize ? (" size: " + header.Size.ToString()) : "");
        //    stringBuilder.Append(header.HasStatus ? (" status: " + header.Status.ToString()) : "");
        //    stringBuilder.AppendLine();
        //    return stringBuilder;
        //}
        //public void LogIncoming(GameMessage msg)
        //{
        //    this.Log(Logger.Level.Dump, "[I] " + msg.AsText(), null);
        //}
        //public void LogOutgoing(GameMessage msg)
        //{
        //    this.Log(Logger.Level.Dump, "[O] " + msg.AsText(), null);
        //}
        public void TraceException(System.Exception exception, string message)
        {
            this.LogException(Logger.Level.Trace, message, null, exception);
        }
        public void TraceException(System.Exception exception, string message, params object[] args)
        {
            this.LogException(Logger.Level.Trace, message, args, exception);
        }
        public void DebugException(System.Exception exception, string message)
        {
            this.LogException(Logger.Level.Debug, message, null, exception);
        }
        public void DebugException(System.Exception exception, string message, params object[] args)
        {
            this.LogException(Logger.Level.Debug, message, args, exception);
        }
        public void InfoException(System.Exception exception, string message)
        {
            this.LogException(Logger.Level.Info, message, null, exception);
        }
        public void InfoException(System.Exception exception, string message, params object[] args)
        {
            this.LogException(Logger.Level.Info, message, args, exception);
        }
        public void WarnException(System.Exception exception, string message)
        {
            this.LogException(Logger.Level.Warn, message, null, exception);
        }
        public void WarnException(System.Exception exception, string message, params object[] args)
        {
            this.LogException(Logger.Level.Warn, message, args, exception);
        }
        public void ErrorException(System.Exception exception, string message)
        {
            this.LogException(Logger.Level.Error, message, null, exception);
        }
        public void ErrorException(System.Exception exception, string message, params object[] args)
        {
            this.LogException(Logger.Level.Error, message, args, exception);
        }
        public void FatalException(System.Exception exception, string message)
        {
            this.LogException(Logger.Level.Fatal, message, null, exception);
        }
        public void FatalException(System.Exception exception, string message, params object[] args)
        {
            this.LogException(Logger.Level.Fatal, message, args, exception);
        }
    }
}
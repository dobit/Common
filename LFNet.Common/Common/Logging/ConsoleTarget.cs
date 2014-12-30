namespace LFNet.Common.Logging
{
    public class ConsoleTarget : LogTarget
    {
        public ConsoleTarget(Logger.Level minLevel, Logger.Level maxLevel, bool includeTimeStamps)
        {
            base.MinimumLevel = minLevel;
            base.MaximumLevel = maxLevel;
            base.IncludeTimeStamps = includeTimeStamps;
        }
        public override void LogMessage(Logger.Level level, string logger, string message)
        {
            string text = base.IncludeTimeStamps ? ("[" + System.DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff") + "] ") : "";
            ConsoleTarget.SetForeGroundColor(level);
            System.Console.WriteLine(string.Format("{0}[{1}] [{2}]: {3}", new object[]
                                                                              {
                                                                                  text,
                                                                                  level.ToString().PadLeft(5),
                                                                                  logger,
                                                                                  message
                                                                              }));
        }
        public override void LogException(Logger.Level level, string logger, string message, System.Exception exception)
        {
            string text = base.IncludeTimeStamps ? ("[" + System.DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff") + "] ") : "";
            ConsoleTarget.SetForeGroundColor(level);
            System.Console.WriteLine(string.Format("{0}[{1}] [{2}]: {3} - [Exception] {4}", new object[]
                                                                                                {
                                                                                                    text,
                                                                                                    level.ToString().PadLeft(5),
                                                                                                    logger,
                                                                                                    message,
                                                                                                    exception
                                                                                                }));
        }
        private static void SetForeGroundColor(Logger.Level level)
        {
            switch (level)
            {
                case Logger.Level.Dump:
                    System.Console.ForegroundColor = System.ConsoleColor.DarkGray;
                    break;
                case Logger.Level.Trace:
                    System.Console.ForegroundColor = System.ConsoleColor.DarkGray;
                    break;
                case Logger.Level.Debug:
                    System.Console.ForegroundColor = System.ConsoleColor.Cyan;
                    break;
                case Logger.Level.Info:
                    System.Console.ForegroundColor = System.ConsoleColor.White;
                    break;
                case Logger.Level.Warn:
                    System.Console.ForegroundColor = System.ConsoleColor.Yellow;
                    break;
                case Logger.Level.Error:
                    System.Console.ForegroundColor = System.ConsoleColor.Magenta;
                    break;
                case Logger.Level.Fatal:
                    System.Console.ForegroundColor = System.ConsoleColor.Red;
                    break;
            }
        }
    }
}
using System.Linq;

namespace LFNet.Common.Logging
{
    internal static class LogRouter
    {
        public static void RouteMessage(Logger.Level level, string logger, string message)
        {
            if (LogManager.Enabled)
            {
                if (LogManager.Targets.Count != 0)
                {
                    foreach (LogTarget current in
                        from target in LogManager.Targets
                        where level >= target.MinimumLevel && level <= target.MaximumLevel
                        select target)
                    {
                        current.LogMessage(level, logger, message);
                    }
                }
            }
        }
        public static void RouteException(Logger.Level level, string logger, string message, System.Exception exception)
        {
            if (LogManager.Enabled)
            {
                if (LogManager.Targets.Count != 0)
                {
                    foreach (LogTarget current in
                        from target in LogManager.Targets
                        where level >= target.MinimumLevel && level <= target.MaximumLevel
                        select target)
                    {
                        current.LogException(level, logger, message, exception);
                    }
                }
            }
        }
    }
}
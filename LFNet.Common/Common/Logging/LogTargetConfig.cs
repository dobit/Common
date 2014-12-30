using LFNet.Configuration;

namespace LFNet.Common.Logging
{
    public class LogTargetConfig //: BaseConfig<LogTargetConfig>
    {
        public string LoggerName { get; set; }


        public LogTargetConfig(string loggerName)
        {
            LoggerName = loggerName;
        }

        public bool Enabled { get; set; }
        public string Target { get; set; }
        public bool IncludeTimeStamps { get; set; }
        public string FileName { get; set; }
        public Logger.Level MinimumLevel { get; set; }
        public Logger.Level MaximumLevel { get; set; }
        public bool ResetOnStartup { get; set; }
       
    }

    
}
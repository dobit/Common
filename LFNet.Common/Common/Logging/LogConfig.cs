using LFNet.Configuration;

namespace LFNet.Common.Logging
{
    public sealed class LogConfig : BaseConfig<LogConfig>
    {
        private LogTargetConfig[] _targets = new LogTargetConfig[]
                                                 {
                                                     new LogTargetConfig("ConsoleLog"),
                                                     new LogTargetConfig("ServerLog"),
                                                     new LogTargetConfig("PacketLog")
                                                 };
        public LogTargetConfig[] Targets
        {
            get { return _targets; }
            set { _targets = value; }
        }

        public string LoggingRoot { get; set; }


    }
}
using System;

namespace Assets.Modules.SimpleLogging
{
    public class LogRecord
    {
        private readonly Logger logger;

        private readonly LogLevel level;

        private readonly string msg;

        private readonly DateTime timestamp;

        public LogRecord(Logger logger, LogLevel level, string msg)
        {
            this.logger = logger;
            this.level = level;
            this.msg = msg;

            timestamp = DateTime.UtcNow;
        }

        public string GetLoggerName()
        {
            return logger.Name;
        }

        public LogLevel GetLogLevel()
        {
            return level;
        }

        public string GetMsg()
        {
            return msg;
        }

        public string GetTime()
        {
            return timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

    }
}
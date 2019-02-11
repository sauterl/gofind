using System;

namespace Assets.Modules.SimpleLogging
{
    public class SimpleConsoleLogHandler : LogHandlingProvider
    {

        private LogLevel lvl;

        public SimpleConsoleLogHandler(LogLevel lvl)
        {
            this.lvl = lvl;
        }

        public void Log(LogRecord record)
        {
            Console.WriteLine(LogManager.Format(record));
        }

        public void SetLevel(LogLevel level)
        {
            lvl = level;
        }
    }
}
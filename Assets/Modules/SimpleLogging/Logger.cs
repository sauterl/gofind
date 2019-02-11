namespace Assets.Modules.SimpleLogging
{
    public class Logger
    {
        public LogLevel Level { get; set; }

        private readonly string name;
        public string Name { get { return name; } }

        private readonly LogHandlingProvider handler;

        public Logger(string name, LogHandlingProvider handler)
        {
            this.name = name;
            this.handler = handler;
        }

        public void Log(LogLevel lvl, string msg)
        {
            handler.Log(new LogRecord(this, lvl, msg));
        }

        public void Log(LogLevel lvl, string format, params object[] args)
        {
            Log(lvl, string.Format(format, args));
        }

        public void Trace(string msg)
        {
            Log(LogLevel.TRACE, msg);
        }

        public void Debug(string msg)
        {
            Log(LogLevel.DEBUG, msg);
        }

        public void Debug(string msg, params object[] args)
        {
            Log(LogLevel.DEBUG, msg, args);
        }

        public void Info(string msg)
        {
            Log(LogLevel.INFO, msg);
        }

        public void Warn(string msg)
        {
            Log(LogLevel.WARN, msg);
        }

        public void Error(string msg)
        {
            Log(LogLevel.ERROR, msg);
        }

        public void LogPause(bool pausing)
        {
            Log(LogLevel.INFO, pausing ? "Paused" : "Unpaused");
        }
    }
}

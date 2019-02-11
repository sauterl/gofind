namespace Assets.Modules.SimpleLogging
{
    public class UnityDebugLogHandler : LogHandlingProvider
    {
        private LogLevel lvl;

        public void Log(LogRecord record)
        {
            if (IsLoggable(record.GetLogLevel()))
            {
                UnityEngine.Debug.Log(LogManager.Format(record));
            }
        }

        public UnityDebugLogHandler(LogLevel lvl)
        {
            this.lvl = lvl;
        }

        public void SetLevel(LogLevel level)
        {
            lvl = level;
        }

        private bool IsLoggable(LogLevel lvl)
        {
            return this.lvl.GetValue() <= lvl.GetValue();
        }
    }
}
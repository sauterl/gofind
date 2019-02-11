namespace Assets.Modules.SimpleLogging
{
    public class MultiLogHandler : LogHandlingProvider
    {

        private LogLevel consoleLevel;
        private LogLevel fileLevel;
        private LogLevel stdoutLevel;

        public void Log(LogRecord record)
        {
            unityHander.Log(record);
            fileHandler.Log(record);
            if (consoleHandler != null)
            {
                consoleHandler.Log(record);
            }
        }

        public void SetLevel(LogLevel level)
        {
            consoleLevel = level;
        }

        public MultiLogHandler(LogLevel consoleLevel, UnityDebugLogHandler consoleHandler, LogLevel fileLevel,
            FileLogHandler fileHandler)
        {
            this.consoleLevel = consoleLevel;
            unityHander = consoleHandler;

            this.fileHandler = fileHandler;
            this.fileLevel = fileLevel;
        }

        public void SetConsoleHandler(SimpleConsoleLogHandler handler)
        {
            consoleHandler = handler;
        }

        private UnityDebugLogHandler unityHander;
        private FileLogHandler fileHandler;
        private SimpleConsoleLogHandler consoleHandler;

    }
}
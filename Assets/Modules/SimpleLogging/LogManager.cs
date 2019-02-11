using System;

namespace Assets.Modules.SimpleLogging
{
    public class LogManager
    {

        private static LogManager instance = null;

        public static LogManager GetInstance()
        {
            if (instance == null)
            {
                instance = new LogManager();;
            }
            return instance;
        }

        private readonly UnityDebugLogHandler unityHandler = new UnityDebugLogHandler(LogLevel.DEBUG);
        private readonly FileLogHandler fileHandler = new FileLogHandler(LogLevel.DEBUG);
        private readonly SimpleConsoleLogHandler consoleHandler = new SimpleConsoleLogHandler(LogLevel.DEBUG);
        private readonly MultiLogHandler multiHandler;

        private LogManager()
        {
            multiHandler = new MultiLogHandler(LogLevel.DEBUG, unityHandler, LogLevel.DEBUG, fileHandler);
            //multiHandler.SetConsoleHandler(consoleHandler);
        }

        public Logger GetLogger(Type type)
        {
            return new Logger(type.Name, multiHandler);
        }

        public void Close()
        {
            fileHandler.CloseWriter();
        }

        public void Reopen()
        {
            fileHandler.ReopenFile();
        }

        public static string Format(LogRecord record, bool includeDate = false)
        {
            return string.Format((includeDate ? "["+ record.GetTime() + "]":"")+"[{0}] {1} - {2}", record.GetLoggerName(), record.GetLogLevel(), record.GetMsg());
        }
    }
}
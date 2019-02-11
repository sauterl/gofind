namespace Assets.Modules.SimpleLogging
{
    public interface LogHandlingProvider
    {

        void Log(LogRecord record);

        void SetLevel(LogLevel level);
    }
}
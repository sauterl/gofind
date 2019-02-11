namespace Assets.Modules.SimpleLogging
{
    public class LogLevel
    {

        public static readonly LogLevel TRACE = new LogLevel("trace", 100);
        public static readonly LogLevel DEBUG = new LogLevel("debug", 300);
        public static readonly LogLevel INFO = new LogLevel("info", 500);
        public static readonly LogLevel WARN = new LogLevel("warn", 700);
        public static readonly LogLevel ERROR = new LogLevel("error", 900);

        private readonly int value;
        private readonly string name;

        public int GetValue()
        {
            return value;
        }

        public string GetName()
        {
            return name;
        }

        private LogLevel(string name, int value)
        {
            this.name = name;
            this.value = value;
        }

        public override string ToString()
        {
            return name.ToUpper();
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(LogLevel))
            {
                LogLevel other = (LogLevel) obj;
                return value == other.value;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return name.GetHashCode() + value.GetHashCode();
        }
    }
}
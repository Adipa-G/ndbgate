using System.Reflection;
using log4net;

namespace DbGate
{
    public class Logger
    {
        public static ILog GetLogger(string loggerName)
        {
            return LogManager.GetLogger(Assembly.GetEntryAssembly(), loggerName);
        }
    }
}

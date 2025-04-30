using log4net;
using System.Reflection;

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
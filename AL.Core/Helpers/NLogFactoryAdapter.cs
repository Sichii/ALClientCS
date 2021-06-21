using System.Reflection;
using Common.Logging;
using Common.Logging.Factory;
using Common.Logging.NLog;
using NLog;
using LogManager = Common.Logging.LogManager;

namespace AL.Core.Helpers
{
    public class NLogFactoryAdapter : AbstractCachingLoggerFactoryAdapter
    {
        public static void ConfigureLogging() => LogManager.Adapter = new NLogFactoryAdapter();

        protected override ILog CreateLogger(string name) =>
            (ILog) typeof(NLogLogger)
                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(Logger) }, null)
                ?.Invoke(new object[] { NLog.LogManager.GetLogger(name) });
    }
}
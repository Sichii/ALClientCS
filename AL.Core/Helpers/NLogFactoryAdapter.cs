using System;
using System.Reflection;
using Common.Logging;
using Common.Logging.Factory;
using Common.Logging.NLog;

namespace AL.Core.Helpers
{
    public class NLogFactoryAdapter : AbstractCachingLoggerFactoryAdapter
    {
        protected override ILog CreateLogger(string name) =>
            (ILog)typeof(NLogLogger).GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(NLog.Logger) }, null)
                ?.Invoke(new object[] { NLog.LogManager.GetLogger(name) });

        public static void ConfigureLogging() => LogManager.Adapter = new NLogFactoryAdapter();
    }
}
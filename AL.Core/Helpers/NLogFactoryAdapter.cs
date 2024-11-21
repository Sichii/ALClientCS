#region
using System.Reflection;
using Common.Logging;
using Common.Logging.Factory;
using Common.Logging.NLog;
using NLog;
using LogManager = Common.Logging.LogManager;
#endregion

namespace AL.Core.Helpers;

/// <summary>
///     Provides an implementation of <see cref="AbstractCachingLoggerFactoryAdapter" /> for NLog
/// </summary>
/// <seealso cref="Common.Logging.Factory.AbstractCachingLoggerFactoryAdapter" />
public sealed class NLogFactoryAdapter : AbstractCachingLoggerFactoryAdapter
{
    /// <summary>
    ///     Configures the underlying <see cref="Common.Logging.LogManager" /> to use this adapter.
    /// </summary>
    public static void ConfigureLogging() => LogManager.Adapter = new NLogFactoryAdapter();

    /// <inheritdoc />
    protected override ILog CreateLogger(string name)
        => (ILog)typeof(NLogLogger).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(Logger)],
            null) !.Invoke([NLog.LogManager.GetLogger(name)]);
}
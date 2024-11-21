#region
using AL.Core.Helpers;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
#endregion

namespace AL.Client;

/// <summary>
///     Represents the configurable options for <see cref="ALClient" />.
/// </summary>
public static class ALClientSettings
{
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    /// <summary>
    ///     The network timeout in milliseconds used for most socket calls. Defaults to 1000.
    ///     <br />
    ///     If you live in narnia and are experiencing timeout exceptions, set it higher.
    /// </summary>
    public static int NetworkTimeoutMS { get; set; } = 1500;

    /// <summary>
    ///     The rate (per second) to update entity positions. Default 30.
    ///     <br />
    ///     If you are experiencing innaccuracies with distance or bounding calculations, set it higher.
    /// </summary>
    public static int PositionPollingRate { get; set; } = 30;

    /// <summary>
    ///     If using <see cref="UseDefaultLoggingConfiguration" />, this will set the minimum logging level for all NLog
    ///     targets that fall under the "AL." namespace.
    /// </summary>
    /// <param name="level">
    ///     The minimum log level verbosity to see.
    /// </param>
    public static void SetLogLevel(LogLevel level)
    {
        foreach (var rule in LogManager.Configuration.LoggingRules)
            if (rule.LoggerNamePattern == "AL.*")
                rule.SetLoggingLevels(level, LogLevel.Fatal);
    }

    /// <summary>
    ///     Configures ALClient to use default logging. This uses <see cref="NLogFactoryAdapter" /> to adapt Common.Logging to
    ///     use an NLog logger, and configures console and file log targets.
    ///     <br />
    ///     Optionally, you can use your own Common.Logging adapter, and configure your own logging.
    /// </summary>
    public static void UseDefaultLoggingConfiguration()
    {
        NLogFactoryAdapter.ConfigureLogging();

        var config = new LoggingConfiguration();

        var fileTarget = new FileTarget("ALClientCSInternalFileTarget")
        {
            Layout = new SimpleLayout(@"[${date:format=HH\:mm\:ss.fff}][${level:uppercase=true}][${logger:shortName=true}] ${message}"),
            FileName = @"logs\${shortdate}.txt",
            ArchiveFileName = @"logs\old\${shortdate}.txt",
            ArchiveEvery = FileArchivePeriod.Day,
            ArchiveNumbering = ArchiveNumberingMode.Rolling,
            MaxArchiveFiles = 30
        };

        var consoleTarget = new ConsoleTarget("ALClientCSConsoleInternalTarget")
        {
            Layout = @"[${level:uppercase=true}][${logger:shortName=true}] ${message}"
        };

        config.AddTarget(fileTarget);
        config.AddTarget(consoleTarget);

        config.AddRule(
            LogLevel.Info,
            LogLevel.Fatal,
            fileTarget,
            "AL.*");

        config.AddRule(
            LogLevel.Info,
            LogLevel.Fatal,
            consoleTarget,
            "AL.*",
            true);
        config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);
        config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);

        LogManager.Configuration = config;
    }
}
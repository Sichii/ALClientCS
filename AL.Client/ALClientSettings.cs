#region
using AL.Core.Helpers;
using AL.SocketClient;
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
    /// <summary>
    ///     The time, in milliseconds, a bank door is given to land once the server has answered it "in progress". Defaults to
    ///     10000.
    /// </summary>
    /// <remarks>
    ///     The server loads or saves the bank before it sends the map change, which takes two to five seconds, past
    ///     <see cref="NetworkTimeoutMS" />.
    /// </remarks>
    public static int BankCrossingTimeoutMS { get; set; } = 10000;

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
    ///     Whether a generated dungeon floor arrives with its tiles and sprite placements. Default true.
    ///     <br />
    ///     Set it to false if nothing draws the map, to receive only the floor's collision lines.
    /// </summary>
    /// <remarks>
    ///     Stored on <see cref="ALSocketClient.ReceiveGeneratedMapArt" />, which reads it when a socket connects.
    /// </remarks>
    public static bool ReceiveGeneratedMapArt
    {
        get => ALSocketClient.ReceiveGeneratedMapArt;
        set => ALSocketClient.ReceiveGeneratedMapArt = value;
    }

    /// <summary>
    ///     If using <see cref="UseDefaultLoggingConfiguration" />, this will set the minimum logging level for all NLog
    ///     targets that fall under the "AL." namespace.
    /// </summary>
    /// <param name="level">The minimum log level verbosity to see.</param>
    public static void SetLogLevel(LogLevel level)
    {
        var configuration = LogManager.Configuration;

        //there are no rules to adjust until something has configured NLog
        if (configuration == null)
            return;

        foreach (var rule in configuration.LoggingRules)
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
            Layout = new SimpleLayout(
                @"[${date:format=HH\:mm\:ss.fff}][${level:uppercase=true}][${logger:shortName=true}] ${message}${onexception:inner=${newline}${exception:format=ToString}}"),
            FileName = @"logs\${shortdate}.txt",
            ArchiveFileName = @"logs\old\${shortdate}.txt",
            ArchiveEvery = FileArchivePeriod.Day,
            MaxArchiveFiles = 30
        };

        var consoleTarget = new ConsoleTarget("ALClientCSConsoleInternalTarget")
        {
            Layout
                = @"[${level:uppercase=true}][${logger:shortName=true}] ${message}${onexception:inner=${newline}${exception:format=ToString}}"
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
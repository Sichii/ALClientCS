using System;
using System.Threading.Tasks;
using AL.Core.Helpers;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CA2211
// ReSharper disable ConvertToConstant.Global

namespace AL.Client
{
    public static class ALClientSettings
    {
        public static readonly ParallelOptions PARALLEL_OPTIONS = new()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };
        public static int NETWORK_TIMEOUT_MS = 2000;

        public static void SetLogLevel(LogLevel level)
        {
            foreach (var rule in LogManager.Configuration.LoggingRules)
                if (rule.LoggerNamePattern == "AL.*")
                    rule.SetLoggingLevels(level, LogLevel.Fatal);
        }

        public static void UseDefaultLoggingConfiguration()
        {
            NLogFactoryAdapter.ConfigureLogging();

            var config = new LoggingConfiguration();
            var fileTarget = new FileTarget("ALClientCSFileTarget")
            {
                Layout = new SimpleLayout(
                    @"[${date:format=HH\:mm\:ss}][${level:uppercase=true}][${logger:shortName=true}] ${message}"),
                FileName = @"logs\${shortdate}.txt",
                ArchiveFileName = @"logs\old\${shortdate}.txt",
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                MaxArchiveFiles = 30
            };

            var consoleTarget = new ConsoleTarget("ALClientCSConsoleTarget")
            {
                Layout = @"[${level:uppercase=true}][${logger:shortName=true}] ${message}"
            };

            config.AddTarget(fileTarget);
            config.AddTarget(consoleTarget);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, "ALClientCSFileTarget", "AL.*");
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, "ALClientCSConsoleTarget", "AL.*");

            LogManager.Configuration = config;
        }
    }
}
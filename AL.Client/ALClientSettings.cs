using System;
using System.Threading.Tasks;
using AL.Core.Helpers;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace ALClientCS
{
    public static class ALClientSettings
    {
        public static readonly ParallelOptions ParallelOptions = new()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

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
                    @"[${date:format=yyyy-MM-dd HH\:mm\:ss}][${level:uppercase=true}][${logger:shortName=true}] ${message}"),
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
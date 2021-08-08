using System;
using AL.Core.Interfaces;
using Common.Logging;

namespace AL.Core.Helpers
{
    /// <summary>
    ///     Provides an abstraction to force the derived to include a name when logging.
    /// </summary>
    public class FormattedLogger : IFormattedLogger
    {
        private readonly ILog Logger;

        public string Prefix { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FormattedLogger" /> class.
        /// </summary>
        /// <param name="prefix">The name to use for logging.</param>
        /// <param name="logger">A Common.Logging logger</param>
        public FormattedLogger(string prefix, ILog logger)
        {
            Logger = logger;
            Prefix = prefix;
        }

        public void Debug(object message) => Logger.Debug($"[{Prefix}] {message}");

        public void Error(object message) =>
            Logger.Error($"[{Prefix}]{Environment.NewLine}{message}{Environment.NewLine}");

        public void Fatal(object message) =>
            Logger.Fatal($"[{Prefix}]{Environment.NewLine}{message}{Environment.NewLine}");

        public void Info(object message) => Logger.Info($"[{Prefix}] {message}");

        public void Trace(object message) => Logger.Trace($"[{Prefix}] {message}");

        public void Warn(object message) => Logger.Warn($"[{Prefix}] {message}");
    }
}
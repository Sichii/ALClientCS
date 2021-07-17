using Common.Logging;

namespace AL.Core.Abstractions
{
    /// <summary>
    ///     Provides an abstraction to force the derived to include a name when logging.
    /// </summary>
    public abstract class NamedLoggerBase
    {
        private readonly ILog Logger;

        /// <summary>
        ///     The name to use for logging.
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NamedLoggerBase" /> class.
        /// </summary>
        /// <param name="name">The name to use for logging.</param>
        protected NamedLoggerBase(string name)
        {
            Logger = LogManager.GetLogger(GetType());
            Name = name;
        }

        /// <summary>
        ///     Logs a debug message by calling <see cref="object.ToString" /> on the message.
        /// </summary>
        /// <param name="message">The object message.</param>
        public void Debug(object message) => Logger.Debug($"[{Name}] {message}");

        /// <summary>
        ///     Logs an error message by calling <see cref="object.ToString" /> on the message.
        /// </summary>
        /// <param name="message">The object message.</param>
        public void Error(object message) => Logger.Error($"[{Name}] {message}");

        /// <summary>
        ///     Logs a fatal message by calling <see cref="object.ToString" /> on the message.
        /// </summary>
        /// <param name="message">The object message.</param>
        public void Fatal(object message) => Logger.Fatal($"[{Name}] {message}");

        /// <summary>
        ///     Logs an info message by calling <see cref="object.ToString" /> on the message.
        /// </summary>
        /// <param name="message">The object message.</param>
        public void Info(object message) => Logger.Info($"[{Name}] {message}");

        /// <summary>
        ///     Logs a trace message by calling <see cref="object.ToString" /> on the message.
        /// </summary>
        /// <param name="message">The object message.</param>
        public void Trace(object message) => Logger.Trace($"[{Name}] {message}");

        /// <summary>
        ///     Logs a warn message by calling <see cref="object.ToString" /> on the message.
        /// </summary>
        /// <param name="message">The object message.</param>
        public void Warn(object message) => Logger.Warn($"[{Name}] {message}");
    }
}
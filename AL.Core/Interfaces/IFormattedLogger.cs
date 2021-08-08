namespace AL.Core.Interfaces
{
    /// <summary>
    ///     Provides an interface for logging messages with a prefix.
    /// </summary>
    public interface IFormattedLogger
    {
        /// <summary>
        ///     Logs a debug message by calling <see cref="object.ToString" /> on the message.
        /// </summary>
        /// <param name="message">The object message.</param>
        void Debug(object message);

        /// <summary>
        ///     Logs an error message by calling <see cref="object.ToString" /> on the message.
        /// </summary>
        /// <param name="message">The object message.</param>
        void Error(object message);

        /// <summary>
        ///     Logs a fatal message by calling <see cref="object.ToString" /> on the message.
        /// </summary>
        /// <param name="message">The object message.</param>
        void Fatal(object message);

        /// <summary>
        ///     Logs an info message by calling <see cref="object.ToString" /> on the message.
        /// </summary>
        /// <param name="message">The object message.</param>
        void Info(object message);

        /// <summary>
        ///     Logs a trace message by calling <see cref="object.ToString" /> on the message.
        /// </summary>
        /// <param name="message">The object message.</param>
        void Trace(object message);

        /// <summary>
        ///     Logs a warn message by calling <see cref="object.ToString" /> on the message.
        /// </summary>
        /// <param name="message">The object message.</param>
        void Warn(object message);
    }
}
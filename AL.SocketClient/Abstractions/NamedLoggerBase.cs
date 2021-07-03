using AL.SocketClient.Interfaces;
using Common.Logging;

namespace AL.SocketClient.Abstractions
{
    public abstract class NamedLoggerBase : INamedLogger
    {
        public abstract ILog Logger { get; init; }
        public abstract string Name { get; init; }
        public void Debug(object message) => Logger.Debug($"[{Name}] {message}");
        public void Error(object message) => Logger.Error($"[{Name}] {message}");
        public void Fatal(object message) => Logger.Fatal($"[{Name}] {message}");
        public void Info(object message) => Logger.Info($"[{Name}] {message}");
        public void Trace(object message) => Logger.Trace($"[{Name}] {message}");

        public void Warn(object message) => Logger.Warn($"[{Name}] {message}");
    }
}
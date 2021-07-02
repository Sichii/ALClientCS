using Common.Logging;

namespace AL.SocketClient.Interfaces
{
    public interface INamedLogger
    {
        ILog Logger { get; }
        string Name { get; }
        void Warn(object message);
        void Trace(object message);
        void Debug(object message);
        void Error(object message);
        void Fatal(object message);
        void Info(object message);
    }
}
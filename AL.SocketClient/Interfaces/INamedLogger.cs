using Common.Logging;

namespace AL.SocketClient.Interfaces
{
    public interface INamedLogger
    {
        ILog Logger { get; }
        string Name { get; }
        void Debug(object message);
        void Error(object message);
        void Fatal(object message);
        void Info(object message);
        void Trace(object message);
        void Warn(object message);
    }
}
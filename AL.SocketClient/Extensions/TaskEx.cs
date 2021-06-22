using System.Threading.Tasks;
using AL.SocketClient.ClientModel;

namespace AL.SocketClient.Extensions
{
    public static class TaskEx
    {
        public static async Task<Expectation<bool?>> Timeout<T>(this Task<T> task, int timeoutMS)
        {
            if (task == await Task.WhenAny(task, Task.Delay(timeoutMS)))
                return true;

            return "Operation timed out";
        }
    }
}
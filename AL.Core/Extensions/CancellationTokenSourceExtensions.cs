using System.Threading;
using System.Threading.Tasks;

namespace AL.Core.Extensions
{
    public static class CancellationTokenSourceExtensions
    {
        public static void CancelWithAsynchronousContinuations(this CancellationTokenSource source) => _ = Task.Run(source.Cancel);
    }
}
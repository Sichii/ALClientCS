using System.Threading;

namespace AL.Core.Helpers
{
    public static class UniqueId
    {
        private static long Id = 0;
        public static long NextId => Interlocked.Increment(ref Id);
    }
}
using System.Diagnostics;

namespace AL.Core.Helpers
{
    public static class DeltaTime
    {
        private static readonly Stopwatch Stopwatch = Stopwatch.StartNew();
        public static long Value => Stopwatch.ElapsedMilliseconds;
    }
}
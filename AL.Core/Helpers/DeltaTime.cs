using System.Diagnostics;

namespace AL.Core.Helpers
{
    /// <summary>
    ///     Provides an easy way to obtain a high-precision time-based value.
    /// </summary>
    public static class DeltaTime
    {
        private static readonly Stopwatch Stopwatch = Stopwatch.StartNew();

        /// <summary>
        ///     Gets the current time value in milliseconds.
        /// </summary>
        public static long Value => Stopwatch.ElapsedMilliseconds;
    }
}
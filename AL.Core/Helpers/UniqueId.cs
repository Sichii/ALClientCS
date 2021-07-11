using System.Threading;

namespace AL.Core.Helpers
{
    /// <summary>
    ///     Provides an easy way to fetch unique Ids in a thread safe way.
    /// </summary>
    public static class UniqueId
    {
        private static long Id;

        /// <summary>
        ///     Gets the next identifier.
        /// </summary>
        /// <value>The next identifier.</value>
        public static long NextId => Interlocked.Increment(ref Id);
    }
}
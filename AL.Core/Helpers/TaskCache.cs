using System.Threading.Tasks;

namespace AL.Core.Helpers
{
    /// <summary>
    ///     Provides cached instances of commonly used result tasks.
    /// </summary>
    public static class TaskCache
    {
        /// <summary>
        ///     Equivalent to <c>Task.CompletedTask</c>
        /// </summary>
        public static readonly Task COMPLETED = Task.CompletedTask;

        /// <summary>
        ///     Equivalent to <c>Task.FromResult(false)</c>
        /// </summary>
        public static readonly Task<bool> FALSE = Task.FromResult(false);
        /// <summary>
        ///     Equivalent to <c>Task.FromResult(true)</c>
        /// </summary>
        public static readonly Task<bool> TRUE = Task.FromResult(true);
    }
}
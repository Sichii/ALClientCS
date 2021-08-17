using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AL.Client.Helpers
{
    /// <summary>
    ///     Generic static helper for doing a shallow copy from one object to another.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    public static class ShallowMerge<T>
    {
        private static readonly Action<T, T> AssignmentDelegate;

        static ShallowMerge()
        {
            if (AssignmentDelegate == null)
            {
                var fromEx = Expression.Parameter(typeof(T), "fromObj");
                var targetEx = Expression.Parameter(typeof(T), "targetObj");

                var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(p => p.CanRead && p.CanWrite)
                    .ToArray();

                var assignmentExpressions =
                    properties.Select(p => Expression.Assign(Expression.Property(targetEx, p), Expression.Property(fromEx, p)));

                AssignmentDelegate = Expression.Lambda<Action<T, T>>(Expression.Block(assignmentExpressions), fromEx, targetEx).Compile();
            }
        }

        /// <summary>
        ///     Merges all (public/non-public) instanced properties from <paramref name="fromObj" /> into
        ///     <paramref name="targetObj" /> <br />
        ///     The first time this runs (for each type), an expression tree will be compiled and stored.
        /// </summary>
        /// <param name="fromObj">The object to merge from.</param>
        /// <param name="targetObj">The object to merge into.</param>
        /// <exception cref="ArgumentNullException">fromObj</exception>
        /// <exception cref="ArgumentNullException">targetObj</exception>
        public static void Merge(T fromObj, T targetObj)
        {
            if (fromObj == null)
                throw new ArgumentNullException(nameof(fromObj));

            if (targetObj == null)
                throw new ArgumentNullException(nameof(targetObj));

            AssignmentDelegate(fromObj, targetObj);
        }
    }
}
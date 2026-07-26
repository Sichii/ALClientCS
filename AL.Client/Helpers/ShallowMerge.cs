#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     Generic static helper for doing a shallow copy from one object to another.
/// </summary>
/// <typeparam name="T">
///     The type of the object. Must be a reference type;
///     a struct target would be assigned by value and the merge discarded.
/// </typeparam>
public static class ShallowMerge<T> where T : class
{
    private static readonly Action<T, T> AssignmentDelegate;

    static ShallowMerge()
    {
        if (AssignmentDelegate == null)
        {
            var fromEx = Expression.Parameter(typeof(T), "fromObj");
            var targetEx = Expression.Parameter(typeof(T), "targetObj");

            var properties = GetRecursiveProperties(typeof(T));

            //typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            //.Where(p => p.CanRead && p.CanWrite)
            //.ToArray();

            var assignmentExpressions = properties.Select(p => Expression.Assign(
                Expression.Property(targetEx, p),
                Expression.Property(fromEx, p)));

            AssignmentDelegate = Expression.Lambda<Action<T, T>>(Expression.Block(assignmentExpressions), fromEx, targetEx)
                                           .Compile();
        }
    }

    private static IEnumerable<PropertyInfo> GetRecursiveProperties(Type type)
        => !type.IsInterface
            ? type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                  .Where(p => p.CanRead && p.CanWrite)
            : new[]
                {
                    type
                }.Concat(type.GetInterfaces())
                 .SelectMany(i => i.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                 .Where(p => p.CanRead && p.CanWrite)
                 .DistinctBy(p => p.Name);

    /// <summary>
    ///     Merges all (public/non-public) instanced properties from <paramref name="fromObj" /> into
    ///     <paramref name="targetObj" />
    ///     <br />
    ///     The first time this runs (for each type), an expression tree will be compiled and stored.
    /// </summary>
    /// <param name="fromObj">
    ///     The object to merge from.
    /// </param>
    /// <param name="targetObj">
    ///     The object to merge into.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     fromObj
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     targetObj
    /// </exception>
    public static void Merge(T fromObj, T targetObj)
    {
        ArgumentNullException.ThrowIfNull(fromObj);
        ArgumentNullException.ThrowIfNull(targetObj);

        AssignmentDelegate(fromObj, targetObj);
    }
}
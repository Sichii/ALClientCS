#region
using System;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using Chaos.Extensions.Common;
#endregion

namespace AL.Core.Extensions;

/// <summary>
///     Provides a set of extensions for <see cref="IInstancedLocation" />s.
/// </summary>
public static class InstancedLocationExtensions
{
    /// <summary>
    ///     <inheritdoc cref="PointExtensions.AngularRelationTo" />
    ///     <br />
    ///     Additionally checks both locations are on the same map.
    /// </summary>
    /// <param name="l1">
    ///     A location.
    /// </param>
    /// <param name="l2">
    ///     Another location.
    /// </param>
    /// <returns>
    ///     <inheritdoc cref="PointExtensions.AngularRelationTo" />
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     l1
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     l2
    /// </exception>
    public static float AngularRelationTo(this IInstancedLocation l1, IInstancedLocation l2)
    {
        ArgumentNullException.ThrowIfNull(l1);

        ArgumentNullException.ThrowIfNull(l2);

        return !l1.InSameInstanceAs(l2) ? float.MaxValue : ((ILocation)l1).AngularRelationTo(l2);
    }

    /// <summary>
    ///     <inheritdoc cref="PointExtensions.DirectionalRelationTo" />
    ///     <br />
    ///     Additionally checks both locations are on the same map.
    /// </summary>
    /// <param name="l1">
    ///     A location.
    /// </param>
    /// <param name="l2">
    ///     Another location.
    /// </param>
    /// <returns>
    ///     <inheritdoc cref="PointExtensions.DirectionalRelationTo" />
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     l1
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     l2
    /// </exception>
    public static Direction DirectionalRelationTo(this IInstancedLocation l1, IInstancedLocation l2)
    {
        ArgumentNullException.ThrowIfNull(l1);

        ArgumentNullException.ThrowIfNull(l2);

        return !l1.InSameInstanceAs(l2) ? Direction.Invalid : ((ILocation)l1).DirectionalRelationTo(l2);
    }

    /// <summary>
    ///     <inheritdoc cref="PointExtensions.Distance" />
    ///     <br />
    ///     Additionally checks both locations are on the same map and instance.
    /// </summary>
    /// <param name="l1">
    ///     A location.
    /// </param>
    /// <param name="l2">
    ///     Another location.
    /// </param>
    /// <returns>
    ///     <inheritdoc cref="PointExtensions.Distance" />
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     l1
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     l2
    /// </exception>
    public static float DistanceWithInstanceCheck(this IInstancedLocation l1, IInstancedLocation l2)
    {
        ArgumentNullException.ThrowIfNull(l1);

        ArgumentNullException.ThrowIfNull(l2);

        return !l1.InSameInstanceAs(l2) ? float.MaxValue : l1.DistanceWithMapCheck(l2);
    }

    /// <summary>
    ///     Checks if two instanced locations are in the same instance.
    /// </summary>
    /// <param name="l1">
    ///     An instanced location.
    /// </param>
    /// <param name="l2">
    ///     Another instanced location.
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>
    ///         true
    ///     </c>
    ///     if both instanced locations are in the same instance, or either location's instance is equal to
    ///     <c>
    ///         string.Empty
    ///     </c>
    ///     , otherwise
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     l1
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     l2
    /// </exception>
    public static bool InSameInstanceAs(this IInstancedLocation l1, IInstancedLocation l2)
    {
        ArgumentNullException.ThrowIfNull(l1);

        ArgumentNullException.ThrowIfNull(l2);

        if ((l1.In == string.Empty) || (l2.In == string.Empty))
            return true;

        if (l1.In is null && l2.In is null)
            return true;

        return l1.In!.EqualsI(l2.In!);
    }

    /// <summary>
    ///     <inheritdoc cref="PointExtensions.OffsetTowards" />
    ///     <br />
    ///     Additionally checks both locations are on the same map.
    /// </summary>
    /// <param name="l1">
    ///     A location.
    /// </param>
    /// <param name="l2">
    ///     Another location.
    /// </param>
    /// <param name="maxDistance">
    ///     The max distance to translate by.
    /// </param>
    /// <returns>
    ///     <inheritdoc cref="PointExtensions.OffsetTowards" />
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     l1
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     l2
    /// </exception>
    public static Point OffsetTowards(this IInstancedLocation l1, IInstancedLocation l2, float maxDistance)
    {
        ArgumentNullException.ThrowIfNull(l1);

        ArgumentNullException.ThrowIfNull(l2);

        return !l1.InSameInstanceAs(l2) ? Point.None : LocationExtensions.OffsetTowards(l1, l2, maxDistance);
    }
}
#region
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
    extension<T>(T location) where T: IInstancedLocation, allows ref struct
    {
        /// <summary>
        ///     The euclidean distance between two instanced locations, or <see cref="float.MaxValue" /> when they are in different
        ///     instances or on different maps.
        /// </summary>
        public float DistanceWithInstanceCheck<T2>(T2 other) where T2: IInstancedLocation, allows ref struct
            => !location.InSameInstanceAs(other) ? float.MaxValue : location.DistanceWithMapCheck(other);

        /// <summary>
        ///     Whether two locations share an instance. An empty instance matches any; a null one is unknown and only matches
        ///     another unknown.
        /// </summary>
        public bool InSameInstanceAs<T2>(T2 other) where T2: IInstancedLocation, allows ref struct
        {
            //read once into locals: null-state analysis does not flow through a property on a generic receiver
            var instance = location.In;
            var otherInstance = other.In;

            if ((instance == string.Empty) || (otherInstance == string.Empty))
                return true;

            //null is an unknown instance rather than a wildcard, so it only matches another unknown
            if (instance is null || otherInstance is null)
                return instance is null && otherInstance is null;

            return instance.EqualsI(otherInstance);
        }
    }

    //kept on the interface: see the note in PointExtensions

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
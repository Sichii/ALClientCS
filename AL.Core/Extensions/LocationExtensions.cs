#region
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using Chaos.Extensions.Common;
#endregion

namespace AL.Core.Extensions;

/// <summary>
///     Provides a set of extensions for <see cref="ILocation" />s.
/// </summary>
public static class LocationExtensions
{
    extension<T>(T location) where T: ILocation, allows ref struct
    {
        /// <summary>
        ///     The euclidean distance between two locations, or <see cref="float.MaxValue" /> when they are on
        ///     different maps.
        /// </summary>
        public float DistanceWithMapCheck<T2>(T2 other) where T2: ILocation, allows ref struct
            => !location.OnSameMapAs(other) ? float.MaxValue : location.Distance(other);

        /// <summary>
        ///     Whether two locations share a map. An empty map name matches any map.
        /// </summary>
        public bool OnSameMapAs<T2>(T2 other) where T2: ILocation, allows ref struct
        {
            if ((location.Map == string.Empty) || (other.Map == string.Empty))
                return true;

            return location.Map is not null && other.Map is not null && location.Map.EqualsI(other.Map);
        }

        /// <summary>
        ///     Creates a new <see cref="Location" /> from this location.
        /// </summary>
        public Location ToLocation() => new(location.Map, location.X, location.Y);
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
    public static float AngularRelationTo(this ILocation l1, ILocation l2)
    {
        ArgumentNullException.ThrowIfNull(l1);

        ArgumentNullException.ThrowIfNull(l2);

        return !l1.OnSameMapAs(l2) ? float.MaxValue : ((IPoint)l1).AngularRelationTo(l2);
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
    public static Direction DirectionalRelationTo(this ILocation l1, ILocation l2)
    {
        ArgumentNullException.ThrowIfNull(l1);

        ArgumentNullException.ThrowIfNull(l2);

        return !l1.OnSameMapAs(l2) ? Direction.Invalid : ((IPoint)l1).DirectionalRelationTo(l2);
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
    public static Point OffsetTowards(this ILocation l1, ILocation l2, float maxDistance)
    {
        ArgumentNullException.ThrowIfNull(l1);

        ArgumentNullException.ThrowIfNull(l2);

        return !l1.OnSameMapAs(l2) ? Point.None : PointExtensions.OffsetTowards(l1, l2, maxDistance);
    }
}
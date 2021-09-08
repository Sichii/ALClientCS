using System;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using Chaos.Core.Extensions;

namespace AL.Core.Extensions
{
    /// <summary>
    ///     Provides a set of extensions for <see cref="ILocation" />s.
    /// </summary>
    public static class LocationExtensions
    {
        /// <summary>
        ///     <inheritdoc cref="PointExtensions.AngularRelationTo" />
        ///     <br />
        ///     Additionally checks both locations are on the same map.
        /// </summary>
        /// <param name="l1">A location.</param>
        /// <param name="l2">Another location.</param>
        /// <returns>
        ///     <inheritdoc cref="PointExtensions.AngularRelationTo" />
        /// </returns>
        /// <exception cref="System.ArgumentNullException">l1</exception>
        /// <exception cref="System.ArgumentNullException">l2</exception>
        public static float AngularRelationTo(this ILocation l1, ILocation l2)
        {
            if (l1 == null)
                throw new ArgumentNullException(nameof(l1));

            if (l2 == null)
                throw new ArgumentNullException(nameof(l2));

            return !l1.OnSameMapAs(l2) ? float.MaxValue : ((IPoint)l1).AngularRelationTo(l2);
        }

        /// <summary>
        ///     <inheritdoc cref="PointExtensions.DirectionalRelationTo" />
        ///     <br />
        ///     Additionally checks both locations are on the same map.
        /// </summary>
        /// <param name="l1">A location.</param>
        /// <param name="l2">Another location.</param>
        /// <returns>
        ///     <inheritdoc cref="PointExtensions.DirectionalRelationTo" />
        /// </returns>
        /// <exception cref="System.ArgumentNullException">l1</exception>
        /// <exception cref="System.ArgumentNullException">l2</exception>
        public static Direction DirectionalRelationTo(this ILocation l1, ILocation l2)
        {
            if (l1 == null)
                throw new ArgumentNullException(nameof(l1));

            if (l2 == null)
                throw new ArgumentNullException(nameof(l2));

            return !l1.OnSameMapAs(l2) ? Direction.Invalid : ((IPoint)l1).DirectionalRelationTo(l2);
        }

        /// <summary>
        ///     <inheritdoc cref="PointExtensions.Distance" />
        ///     <br />
        ///     Additionally checks both locations are on the same map.
        /// </summary>
        /// <param name="l1">A location.</param>
        /// <param name="l2">Another location.</param>
        /// <returns>
        ///     <inheritdoc cref="PointExtensions.Distance" />
        /// </returns>
        /// <exception cref="System.ArgumentNullException">l1</exception>
        /// <exception cref="System.ArgumentNullException">l2</exception>
        public static float Distance(this ILocation l1, ILocation l2)
        {
            if (l1 == null)
                throw new ArgumentNullException(nameof(l1));

            if (l2 == null)
                throw new ArgumentNullException(nameof(l2));

            return !l1.OnSameMapAs(l2) ? float.MaxValue : ((IPoint)l1).Distance(l2);
        }

        /// <summary>
        ///     <inheritdoc cref="PointExtensions.MidPoint" />
        ///     <br />
        ///     Additionally checks both locations are on the same map.
        /// </summary>
        /// <param name="l1">A location.</param>
        /// <param name="l2">Another location.</param>
        /// <returns>
        ///     <inheritdoc cref="PointExtensions.MidPoint" />
        /// </returns>
        /// <exception cref="System.ArgumentNullException">l1</exception>
        /// <exception cref="System.ArgumentNullException">l2</exception>
        public static Point MidPoint(this ILocation l1, ILocation l2)
        {
            if (l1 == null)
                throw new ArgumentNullException(nameof(l1));

            if (l2 == null)
                throw new ArgumentNullException(nameof(l2));

            return !l1.OnSameMapAs(l2) ? Point.None : ((IPoint)l1).MidPoint(l2);
        }

        private static bool OnSameMapAs(this ILocation l1, ILocation l2)
        {
            if ((l1.Map == string.Empty) || (l2.Map == string.Empty))
                return true;

            return l1.Map.EqualsI(l2.Map);
        }

        /// <summary>
        ///     Creates a new <see cref="Location" /> from an <see cref="ILocation" />.
        /// </summary>
        /// <param name="location">The location to get an instances of.</param>
        /// <returns>
        ///     <see cref="Location" /> <br />
        ///     A new location.
        /// </returns>
        /// <exception cref="ArgumentNullException">location</exception>
        public static Location ToLocation(this ILocation location) => location switch
        {
            null         => throw new ArgumentNullException(nameof(location)),
            Location loc => loc,
            _            => new Location(location.Map, location.ToPoint())
        };

        /// <summary>
        ///     <inheritdoc cref="PointExtensions.Translate" />
        ///     <br />
        ///     Additionally checks both locations are on the same map.
        /// </summary>
        /// <param name="l1">A location.</param>
        /// <param name="l2">Another location.</param>
        /// <param name="maxDistance">The max distance to translate by.</param>
        /// <returns>
        ///     <inheritdoc cref="PointExtensions.Translate" />
        /// </returns>
        /// <exception cref="System.ArgumentNullException">l1</exception>
        /// <exception cref="System.ArgumentNullException">l2</exception>
        public static Point Translate(this ILocation l1, ILocation l2, float maxDistance)
        {
            if (l1 == null)
                throw new ArgumentNullException(nameof(l1));

            if (l2 == null)
                throw new ArgumentNullException(nameof(l2));

            return !l1.OnSameMapAs(l2) ? Point.None : ((IPoint)l1).Translate(l2, maxDistance);
        }
    }
}
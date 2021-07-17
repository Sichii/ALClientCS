using System;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using Chaos.Core.Extensions;

namespace AL.Core.Extensions
{
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
        /// <param name="l1">A location.</param>
        /// <param name="l2">Another location.</param>
        /// <returns>
        ///     <inheritdoc cref="PointExtensions.AngularRelationTo" />
        /// </returns>
        /// <exception cref="System.ArgumentNullException">l1</exception>
        /// <exception cref="System.ArgumentNullException">l2</exception>
        public static float AngularRelationTo(this IInstancedLocation l1, IInstancedLocation l2)
        {
            if (l1 == null)
                throw new ArgumentNullException(nameof(l1));

            if (l2 == null)
                throw new ArgumentNullException(nameof(l2));

            return !l1.In.EqualsI(l2.In) ? float.MaxValue : ((ILocation) l1).AngularRelationTo(l2);
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
        public static Direction DirectionalRelationTo(this IInstancedLocation l1, IInstancedLocation l2)
        {
            if (l1 == null)
                throw new ArgumentNullException(nameof(l1));

            if (l2 == null)
                throw new ArgumentNullException(nameof(l2));

            return !l1.In.EqualsI(l2.In) ? Direction.Invalid : ((ILocation) l1).DirectionalRelationTo(l2);
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
        public static float Distance(this IInstancedLocation l1, IInstancedLocation l2)
        {
            if (l1 == null)
                throw new ArgumentNullException(nameof(l1));

            if (l2 == null)
                throw new ArgumentNullException(nameof(l2));

            return !l1.In.EqualsI(l2.In) ? float.MaxValue : ((ILocation) l1).Distance(l2);
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
        public static Point MidPoint(this IInstancedLocation l1, IInstancedLocation l2)
        {
            if (l1 == null)
                throw new ArgumentNullException(nameof(l1));

            if (l2 == null)
                throw new ArgumentNullException(nameof(l2));

            return !l1.In.EqualsI(l2.In) ? Point.None : ((ILocation) l1).MidPoint(l2);
        }

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
        public static Point Translate(this IInstancedLocation l1, IInstancedLocation l2, float maxDistance)
        {
            if (l1 == null)
                throw new ArgumentNullException(nameof(l1));

            if (l2 == null)
                throw new ArgumentNullException(nameof(l2));

            return !l1.In.EqualsI(l2.In) ? Point.None : ((ILocation) l1).Translate(l2, maxDistance);
        }
    }
}
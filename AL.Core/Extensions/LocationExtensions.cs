using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using Chaos.Core.Extensions;

namespace AL.Core.Extensions
{
    public static class LocationExtensions
    {
        public static float AngularRelationTo(this ILocation l1, ILocation l2) =>
            !l1.Map.EqualsI(l2.Map) ? float.MaxValue : ((IPoint) l1).AngularRelationTo(l2);

        public static Direction DirectionalRelationTo(this ILocation l1, ILocation l2) =>
            !l1.Map.EqualsI(l2.Map) ? Direction.Invalid : ((IPoint) l1).DirectionalRelationTo(l2);

        public static float Distance(this ILocation l1, ILocation l2) =>
            !l1.Map.EqualsI(l2.Map) ? float.MaxValue : ((IPoint) l1).Distance(l2);

        public static Point Lerp(this ILocation l1, ILocation l2, float maxMove, float minDiff) =>
            !l1.Map.EqualsI(l2.Map) ? Point.None : ((IPoint) l1).Lerp(l2, maxMove, minDiff);

        public static Point MidPoint(this ILocation l1, ILocation l2) => !l1.Map.EqualsI(l2.Map) ? Point.None : ((IPoint) l1).MidPoint(l2);
    }
}
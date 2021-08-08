using System;
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Interfaces;
using Chaos.Core.Extensions;

namespace AL.Core.Comparers
{
    public class PointEqualityComparer : IEqualityComparer<IPoint>
    {
        public bool Equals(IPoint? p1, IPoint? p2)
        {
            if (ReferenceEquals(p1, p2))
                return true;

            if (ReferenceEquals(p1, default))
                return false;

            if (ReferenceEquals(p2, default))
                return false;

            return p1.X.NearlyEquals(p2.X, CONSTANTS.EPSILON) && p1.Y.NearlyEquals(p2.Y, CONSTANTS.EPSILON);
        }

        public int GetHashCode(IPoint obj) => HashCode.Combine(Convert.ToInt32(obj.X), Convert.ToInt32(obj.Y));
    }
}
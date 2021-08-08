using System;
using System.Collections.Generic;
using AL.Core.Interfaces;

namespace AL.Core.Comparers
{
    public class CircleEqualityComparer : IEqualityComparer<ICircle>
    {
        public bool Equals(ICircle? x, ICircle? y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, null))
                return false;

            if (ReferenceEquals(y, null))
                return false;

            return x.Radius.Equals(y.Radius) && IPoint.Comparer.Equals(x, y);
        }

        public int GetHashCode(ICircle obj) => HashCode.Combine(obj.Radius.GetHashCode(), IPoint.Comparer.GetHashCode(obj));
    }
}
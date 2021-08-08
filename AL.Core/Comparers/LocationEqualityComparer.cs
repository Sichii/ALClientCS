using System;
using System.Collections.Generic;
using AL.Core.Interfaces;
using Chaos.Core.Extensions;

namespace AL.Core.Comparers
{
    public class LocationEqualityComparer : IEqualityComparer<ILocation>
    {
        public bool Equals(ILocation? x, ILocation? y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, default))
                return false;

            if (ReferenceEquals(y, default))
                return false;

            return x.Map.EqualsI(y.Map) && IPoint.Comparer.Equals(x, y);
        }

        public int GetHashCode(ILocation obj) => HashCode.Combine(obj.Map, IPoint.Comparer.GetHashCode(obj));
    }
}
using System;
using System.Collections.Generic;
using AL.Core.Interfaces;
using Chaos.Core.Extensions;

namespace AL.Core.Comparers
{
    public class InstancedLocationEqualityComparer : IEqualityComparer<IInstancedLocation>
    {
        public bool Equals(IInstancedLocation? x, IInstancedLocation? y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, null))
                return false;

            if (ReferenceEquals(y, null))
                return false;

            return x.In.EqualsI(y.In) && ILocation.Comparer.Equals(x, y);
        }

        public int GetHashCode(IInstancedLocation obj) =>
            HashCode.Combine(obj.In.GetHashCode(), ILocation.Comparer.GetHashCode(obj));
    }
}
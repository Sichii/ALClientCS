using System;

namespace AL.Core.Interfaces
{
    /// <summary>
    ///     Provides an interface for specifying a center point, and a distance from that center.
    /// </summary>
    public interface ICircle : IPoint, IEquatable<ICircle>
    {
        float Radius { get; }
    }
}
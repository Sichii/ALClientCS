namespace AL.Core.Interfaces
{
    /// <summary>
    ///     Represents a location on a map.
    /// </summary>
    public interface ILocation : IPoint
    {
        /// <summary>
        ///     The map this object is in.
        /// </summary>
        string Map { get; }
    }
}
namespace AL.Core.Interfaces
{
    /// <summary>
    ///     Represents a location on a map.
    /// </summary>
    public interface ILocation : IPoint
    {
        string Map { get; }
    }
}
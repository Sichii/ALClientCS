namespace AL.Core.Interfaces
{
    /// <summary>
    ///     Represents an object that could potentially be in an instanced map.
    /// </summary>
    /// <seealso cref="ILocation" />
    public interface IInstancedLocation : ILocation
    {
        /// <summary>
        ///     Which instance this object is in. <br />
        ///     If it's a dungeon, it's a unique ID, otherwise it's the <see cref="ILocation.Map" />.
        /// </summary>
        string In { get; }
    }
}
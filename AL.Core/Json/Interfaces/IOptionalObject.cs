namespace AL.Core.Json.Interfaces
{
    /// <summary>
    ///     Represents an object that can be represented as a basic scalar value (string, bool, etc), or an object.
    /// </summary>
    public interface IOptionalObject
    {
        /// <summary>
        ///     Whether or not the object was a scalar value.
        /// </summary>
        /// <value><c>true</c> if object was not a scalar value.; otherwise, <c>false</c>.</value>
        bool ContainsData { get; init; }
    }
}
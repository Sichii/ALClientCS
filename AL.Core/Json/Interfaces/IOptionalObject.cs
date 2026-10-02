namespace AL.Core.Json.Interfaces;

/// <summary>
///     Represents an object that can be represented as a basic scalar value (string, bool, etc), or an object.
/// </summary>
public interface IOptionalObject
{
    /// <summary>
    ///     Whether the server sent a full object rather than a scalar value.
    /// </summary>
    bool ContainsData { get; set; }
}
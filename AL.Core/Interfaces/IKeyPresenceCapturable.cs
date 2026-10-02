namespace AL.Core.Interfaces;

/// <summary>
///     Represents an object that records which wire keys a frame carried, so a merge can tell a sent zero from an omitted
///     key.
/// </summary>
/// <remarks>
///     The server omits a value equal to its game-data default, so a merge that copies every property wipes live state.
/// </remarks>
public interface IKeyPresenceCapturable
{
    /// <summary>
    ///     Records that a key was present on the wire. Called once per top-level key during deserialization, so it must not
    ///     allocate.
    /// </summary>
    /// <param name="key">The wire key.</param>
    void MarkPresent(string key);
}
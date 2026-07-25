namespace AL.Core.Interfaces;

/// <summary>
///     Implemented by objects whose deserializer must record which wire keys a frame actually carried, so a
///     later merge can distinguish "the server sent 0" from "the server omitted this key". The server's entity
///     encoders are sparse - a value equal to the G default is omitted - so a whitelist merge that copies every
///     property unconditionally wipes live state on every partial delta.
/// </summary>
public interface IKeyPresenceCapturable
{
    /// <summary>
    ///     Records that <paramref name="key" /> was present on the wire. Called once per top-level key during
    ///     deserialization. Implementations must be allocation-free on this hot path.
    /// </summary>
    void MarkPresent(string key);
}

#region
using System.Text.Json.Serialization;
using AL.SocketClient.Model;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data received when performing, or getting updates regarding queued actions.
/// </summary>
public sealed record QueuedActionData
{
    /// <summary>
    ///     The inventory slot the queued operation is running in - the one holding its placeholder, and the slot
    ///     <see cref="Prediction" /> below belongs to. It is the server's <c>ref.num</c>
    ///     (<c>node/server.js:13240</c>), not a count of anything.
    /// </summary>
    [JsonPropertyName("num")]
    public int Slot { get; init; }

    /// <summary>
    ///     The in-progress operation's detail, for the item in <see cref="Slot" />. This frame is the only place it is
    ///     ever sent - no inventory or character frame restates it - so a consumer that ignores this never sees the
    ///     operation's roll at all.
    /// </summary>
    [JsonPropertyName("p")]
    public Prediction? Prediction { get; init; }

    /// <summary>
    ///     If populated, contains information about queued actions that are in progress, or just started.
    /// </summary>
    [JsonPropertyName("q")]
    public QueuedActionInfo? QueuedActionInfo { get; init; }
}
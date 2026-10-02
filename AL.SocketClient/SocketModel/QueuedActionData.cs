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
    ///     The in-progress operation's detail, for the item in <see cref="Slot" />. No inventory or character frame restates
    ///     it.
    /// </summary>
    [JsonPropertyName("p")]
    public Prediction? Prediction { get; init; }

    /// <summary>
    ///     If populated, contains information about queued actions that are in progress, or just started.
    /// </summary>
    [JsonPropertyName("q")]
    public QueuedActionInfo? QueuedActionInfo { get; init; }

    /// <summary>
    ///     The inventory slot holding the queued operation's placeholder, which <see cref="Prediction" /> belongs to. It is
    ///     the server's <c>ref.num</c>, not a count.
    /// </summary>
    [JsonPropertyName("num")]
    public int Slot { get; init; }
}
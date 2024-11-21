#region
using AL.SocketClient.Model;
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data received when performing, or getting updates regarding queued actions.
/// </summary>
public sealed record QueuedActionData
{
    /// <summary>
    ///     TODO: unsure, possibly the number of queued actions currently being taken?
    /// </summary>
    public int Num { get; init; }

    /// <summary>
    ///     If populated, no action was actually performed. This object contains information about what would happen if you
    ///     tried to upgrade/compound the item.
    /// </summary>
    [JsonProperty("p")]
    public Prediction? Prediction { get; init; }

    /// <summary>
    ///     If populated, contains information about queued actions that are in progress, or just started.
    /// </summary>
    [JsonProperty("q")]
    public QueuedActionInfo? QueuedActionInfo { get; init; }
}
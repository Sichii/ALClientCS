#region
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents all of the queued actions that can be taken.
/// </summary>
public sealed record QueuedActionInfo
{
    /// <summary>
    ///     If populated, the compound action is being taken.
    ///     <br />
    ///     This object holds information about when it will finish.
    /// </summary>
    [JsonProperty]
    public QueuedAction? Compound { get; init; }

    /// <summary>
    ///     If populated, the exchange action is being taken.
    ///     <br />
    ///     This object holds information about when it will finish.
    /// </summary>
    [JsonProperty]
    public QueuedAction? Exchange { get; init; }

    /// <summary>
    ///     If populated, the upgrade action is being taken.
    ///     <br />
    ///     This object holds information about when it will finish.
    /// </summary>
    [JsonProperty]
    public QueuedAction? Upgrade { get; init; }
}
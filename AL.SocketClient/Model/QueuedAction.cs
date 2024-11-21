#region
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents an action that will take some amount of time.
/// </summary>
public sealed record QueuedAction
{
    /// <summary>
    ///     The amount of time remaining until the action is completed, in milliseconds.
    /// </summary>
    [JsonProperty("ms")]
    public float CurrentMS { get; init; }

    /// <summary>
    ///     The total amount of time it takes for the action to complete, in milliseconds.
    /// </summary>
    [JsonProperty("len")]
    public float LengthMS { get; init; }

    /// <summary>
    ///     TODO: Not sure... possibly if multiple actions are being taken at once, this number denotes the order they were
    ///     taken.
    /// </summary>
    [JsonProperty]
    public int Num { get; init; }
}
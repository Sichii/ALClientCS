#region
using AL.Core.Definitions;
using AL.Core.Json.Converters;
using AL.SocketClient.Interfaces;
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.Model;

public sealed record SimplePlayer : ISimplePlayer
{
    [JsonConverter(typeof(AfkConverter))]
    public bool AFK { get; init; }
    public int Age { get; init; }

    [JsonProperty("type")]
    public ALClass Class { get; init; }

    public int Level { get; init; }
    public string Map { get; init; } = null!;
    public string Name { get; init; } = null!;

    [JsonProperty("party")]
    public string? PartyLeader { get; init; }
}
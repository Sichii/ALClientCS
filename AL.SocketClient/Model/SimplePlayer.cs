#region
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.SocketClient.Interfaces;
using StjConverters = AL.Core.Json.SystemTextJson;
#endregion

namespace AL.SocketClient.Model;

public sealed record SimplePlayer : ISimplePlayer
{
    [JsonConverter(typeof(StjConverters.AfkConverter))]
    public bool AFK { get; init; }

    public int Age { get; init; }

    [JsonPropertyName("type")]
    public ALClass Class { get; init; }

    public int Level { get; init; }
    public string Map { get; init; } = null!;
    public string Name { get; init; } = null!;

    [JsonPropertyName("party")]
    public string? PartyLeader { get; init; }
}
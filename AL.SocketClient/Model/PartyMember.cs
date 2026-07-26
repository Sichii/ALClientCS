#region
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.Core.Interfaces;
using StjConverters = AL.Core.Json.SystemTextJson;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents a member of your party.
/// </summary>
/// <seealso cref="ILocation" />
public record PartyMember : IInstancedLocation
{
    [JsonPropertyName("type")]
    public ALClass Class { get; init; }

    /// <summary>
    ///     This member's cosmetics; only sent when the member has any.
    /// </summary>
    [JsonPropertyName("cx")]
    public CosmeticInfo? Cosmetics { get; init; }

    public int Gold { get; init; }
    public string In { get; init; } = string.Empty;
    public int Level { get; init; }
    public int Luck { get; init; }
    public string Map { get; init; } = string.Empty;

    /// <summary>
    ///     The maximum party size this member can be apart of.
    /// </summary>
    [JsonPropertyName("l")]
    public int PartyLimit { get; init; }

    [JsonPropertyName("pdps")]
    public float PDPS { get; init; }

    /// <summary>
    ///     True when this member is dead. Only sent while the member is dead.
    /// </summary>
    [JsonPropertyName("rip")]
    [JsonConverter(typeof(StjConverters.AfkConverter))]
    public bool RIP { get; init; }

    public float Share { get; init; }
    public string Skin { get; init; } = null!;
    public float X { get; init; }
    public int XP { get; init; }
    public float Y { get; init; }
    public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);

    public virtual bool Equals(ILocation? other) => ILocation.Comparer.Equals(this, other);
}
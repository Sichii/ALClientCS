#region
using System.Text.Json.Serialization;
using AL.Core.Interfaces;
#endregion

namespace AL.APIClient.Model;

/// <summary>
///     Represents a character this user owns.
/// </summary>
/// <seealso cref="IInstancedLocation" />
public record CharacterInfo : IInstancedLocation
{
    /// <summary>
    ///     The id of the character. (this is not the name)
    /// </summary>
    public string Id { get; init; } = null!;

    public string In { get; init; } = null!;

    /// <summary>
    ///     The level of the character.
    /// </summary>
    public int Level { get; init; }

    public string Map { get; init; } = null!;

    /// <summary>
    ///     The name of the character.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     Milliseconds since the character was last seen online; 0 when it is not currently logged in.
    /// </summary>
    public long Online { get; init; }

    /// <summary>
    ///     TODO: unknown
    /// </summary>
    public string? Secret { get; init; }

    /// <summary>
    ///     If the character is <see cref="IsOnline" />, this is the <see cref="AL.APIClient.Definitions.ServerRegion" /> and
    ///     <see cref="AL.APIClient.Definitions.ServerId" />
    /// </summary>
    [JsonPropertyName("server")]
    public string? ServerKey { get; init; }

    public float X { get; init; }
    public float Y { get; init; }

    /// <summary>
    ///     Whether the character is currently logged in.
    /// </summary>
    [JsonIgnore]
    public bool IsOnline => Online != 0;

    public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);

    public virtual bool Equals(ILocation? other) => ILocation.Comparer.Equals(this, other);

    public override string ToString() => IInstancedLocation.ToString(this);
}
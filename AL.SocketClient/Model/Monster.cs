#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents a monster entity.
/// </summary>
/// <seealso cref="EntityBase" />
public class Monster : EntityBase, IEquatable<Monster>
{
    /// <summary>
    ///     Whether kills on this monster count for every attacker, not just the tag holder.
    /// </summary>
    [JsonPropertyName("cooperative")]
    public bool Cooperative { get; init; }

    /// <summary>
    ///     <b>
    ///         NULLABLE.
    ///     </b>
    ///     If populated, a per-monster drop table as raw
    ///     <c>
    ///         [chance, item]
    ///     </c>
    ///     tuples, overriding the type's defaults. Not interpreted by this client.
    /// </summary>
    [JsonPropertyName("drops")]
    public IReadOnlyList<object>? Drops { get; init; }

    /// <summary>
    ///     The name of the monster. (bee, cutebee, mole, etc...)
    /// </summary>
    [JsonPropertyName("type")]
    public string Name { get; init; } = null!;

    /// <summary>
    ///     Whether this monster dies in a single hit regardless of damage dealt.
    /// </summary>
    [JsonPropertyName("1hp")]
    public bool OneHP { get; init; }

    /// <summary>
    ///     <b>
    ///         NULLABLE.
    ///     </b>
    ///     For a pet or trap, the id of the character that owns it.
    /// </summary>
    [JsonPropertyName("owner")]
    public string? Owner { get; init; }

    /// <summary>
    ///     Whether this monster is a summoned pet.
    /// </summary>
    [JsonPropertyName("pet")]
    public bool Pet { get; init; }

    /// <summary>
    ///     <b>
    ///         NULLABLE.
    ///     </b>
    ///     If this monster is a summoned pet, its given display name. Distinct from <see cref="Name" />, which carries the
    ///     monster
    ///     <c>
    ///         type
    ///     </c>
    ///     .
    /// </summary>
    [JsonPropertyName("name")]
    public string? PetName { get; init; }

    /// <summary>
    ///     <b>
    ///         NULLABLE.
    ///     </b>
    ///     If populated, the appearance skin overriding this monster's default sprite.
    /// </summary>
    [JsonPropertyName("skin")]
    public string? Skin { get; init; }

    /// <summary>
    ///     Whether this monster is a placed trap.
    /// </summary>
    [JsonPropertyName("trap")]
    public bool Trap { get; init; }

    public virtual bool Equals(Monster? other) => Name.Equals(other?.Name) && base.Equals(other);

    public override bool Equals(object? obj) => Equals(obj as Monster);

    public override int GetHashCode() => HashCode.Combine(Name.GetHashCode(), base.GetHashCode());
}
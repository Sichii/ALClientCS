#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Sets;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class SetsDatum : DatumBase<GSet>
{
    [JsonPropertyName("bunny")]
    public GSet Bunny { get; init; } = null!;

    [JsonPropertyName("fury")]
    public GSet Fury { get; init; } = null!;

    [JsonPropertyName("holidays")]
    public GSet Holidays { get; init; } = null!;

    [JsonPropertyName("legends")]
    public GSet Legends { get; init; } = null!;

    [JsonPropertyName("mmage")]
    public GSet MMage { get; init; } = null!;

    [JsonPropertyName("mmerchant")]
    public GSet MMerchant { get; init; } = null!;

    [JsonPropertyName("mpx")]
    public GSet MPX { get; init; } = null!;

    [JsonPropertyName("mpriest")]
    public GSet MPriest { get; init; } = null!;

    [JsonPropertyName("mranger")]
    public GSet MRanger { get; init; } = null!;

    [JsonPropertyName("mrogue")]
    public GSet MRogue { get; init; } = null!;

    [JsonPropertyName("mwarrior")]
    public GSet MWarrior { get; init; } = null!;

    [JsonPropertyName("oathkeeper")]
    public GSet Oathkeeper { get; init; } = null!;

    [JsonPropertyName("rugged")]
    public GSet Rugged { get; init; } = null!;

    [JsonPropertyName("swift")]
    public GSet Swift { get; init; } = null!;

    [JsonPropertyName("tiger")]
    public GSet Tiger { get; init; } = null!;

    [JsonPropertyName("vampires")]
    public GSet Vampires { get; init; } = null!;

    [JsonPropertyName("wt3")]
    public GSet WT3 { get; init; } = null!;

    [JsonPropertyName("wt4")]
    public GSet WT4 { get; init; } = null!;

    [JsonPropertyName("wanderers")]
    public GSet Wanderers { get; init; } = null!;

    internal override void BuildLookupTable()
    {
        base.BuildLookupTable();

        //the accessor comes off the key the server used, not the local spelling - the same arrangement ItemsDatum
        //and MonstersDatum use. The lookup files the wire name first, so that is the one an entry keeps
        foreach ((var accessor, var set) in Entries)
            if (string.IsNullOrEmpty(set.Accessor))
                set.Accessor = accessor;
    }
}
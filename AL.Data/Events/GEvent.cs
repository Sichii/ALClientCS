#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Events;

/// <summary>
///     Represents the static data for a timed world event.
/// </summary>
public record GEvent
{
    /// <summary>
    ///     The daily dungeon's camp table, one list per floor in floor order. Null on every other event.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<GCamp>>? Camps { get; init; }

    /// <summary>
    ///     Trick or Treat's doors. A knock names a door by its index here, and has to come from within 80 of it. Null on every
    ///     other event.
    /// </summary>
    public IReadOnlyList<GTrickOrTreatDoor>? Doors { get; init; }

    /// <summary>
    ///     How long the event runs once it starts, in seconds - minutes for the daily and nightly bosses, weeks for a seasonal
    ///     one.
    /// </summary>
    public int Duration { get; init; }

    /// <summary>
    ///     The daily dungeon's encounter table. Null on every other event.
    /// </summary>
    public IReadOnlyList<GEncounter>? Encounters { get; init; }

    /// <summary>
    ///     How long the Witch's Cipher's gathering lasts from the start of a round, in seconds. Zero on every other event.
    /// </summary>
    public int Gathering { get; init; }

    /// <summary>
    ///     The Witch's Cipher's ingredients, in the order the server numbers recipes by. Null on every other event.
    /// </summary>
    public IReadOnlyList<GWitchIngredient>? Ingredients { get; init; }

    /// <summary>
    ///     Whether the event can be joined from wherever the character is standing, rather than only by walking to where it is
    ///     happening. Carried by the daily and nightly bosses; absent, and so false, on every seasonal one.
    /// </summary>
    public bool Join { get; init; }

    /// <summary>
    ///     The daily dungeon's merchant stock: each merchant encounter offers one of these, picked at random. Null on every
    ///     other event.
    /// </summary>
    [JsonPropertyName("merchant_stock")]
    public IReadOnlyList<GStockItem?>? MerchantStock { get; init; }

    /// <summary>
    ///     How many Witch's Cipher mixes an account may brew per UTC day. Zero on every other event.
    /// </summary>
    public int Mixes { get; init; }

    /// <summary>
    ///     The event's display name — "Giga Crab", "Goo Brawl", "A/B Testing".
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     How many ingredients make a Witch's Cipher mix. Zero on every other event.
    /// </summary>
    public int Slots { get; init; }

    /// <summary>
    ///     What the client draws the event as. Usually a monster skin - the boss the event is about, or one of the things it
    ///     brings out - and otherwise an item the event hands out, such as the Lunar New Year's red envelope.
    /// </summary>
    /// <remarks>
    ///     A/B Testing's names a character skin, which is on neither the monster table nor the item table.
    /// </remarks>
    public string? Sprite { get; init; }

    /// <summary>
    ///     The schedule the event runs on - <c>daily</c>, <c>nightly</c> or <c>seasonal</c> - not what kind of content it is:
    ///     the two nightly ones are world bosses, and so is one of the seasonal ones.
    /// </summary>
    public string? Type { get; init; }
}
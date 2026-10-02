#region
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Images;

/// <summary>
///     Represents the wardrobe rules: which names stand for several sprites, which stand for another name, and where the
///     sheets a slot draws from are placed.
/// </summary>
/// <remarks>
///     Every table here is ordinal, because the server's own lookups are case-sensitive. A cosmetic's slot is resolved
///     through <see cref="GSprite.Type" />, never through these catalogues.
/// </remarks>
public sealed record GCosmetics
{
    /// <summary>
    ///     How far a back cosmetic moves sideways when the character faces left or right, keyed by sprite name. A name absent
    ///     here moves 3.
    /// </summary>
    [JsonPropertyName("back")]
    public IReadOnlyDictionary<string, double> Back { get; init; } = new Dictionary<string, double>(StringComparer.Ordinal);

    /// <summary>
    ///     The names that stand for several sprites at once, each mapped to its members. A bundle name is not itself wearable,
    ///     and its members are not sent back through <see cref="Map" />.
    /// </summary>
    [JsonPropertyName("bundle")]
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Bundle { get; init; }
        = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

    /// <summary>
    ///     How far up the body a beard or mask sits, measured from <see cref="DefaultHeadPlace" />.
    /// </summary>
    [JsonPropertyName("default_beard_position")]
    public int DefaultBeardPosition { get; init; }

    /// <summary>
    ///     How far up the body a face sits, measured from <see cref="DefaultHeadPlace" />.
    /// </summary>
    [JsonPropertyName("default_face_position")]
    public int DefaultFacePosition { get; init; }

    /// <summary>
    ///     How far up the body hair sits before the head's and the hair's own offsets move it.
    /// </summary>
    [JsonPropertyName("default_hair_place")]
    public int DefaultHairPlace { get; init; }

    /// <summary>
    ///     How far up the body a hat sits before the head's and the hair's heights move it.
    /// </summary>
    [JsonPropertyName("default_hat_place")]
    public int DefaultHatPlace { get; init; }

    /// <summary>
    ///     How far up the body a head sits before <see cref="GSprite.Size" /> moves it, in the sprite's own pixels rather than
    ///     screen ones.
    /// </summary>
    [JsonPropertyName("default_head_place")]
    public int DefaultHeadPlace { get; init; }

    /// <summary>
    ///     How far up the body makeup sits, measured from <see cref="DefaultHeadPlace" />.
    /// </summary>
    [JsonPropertyName("default_makeup_position")]
    public int DefaultMakeupPosition { get; init; }

    /// <summary>
    ///     The gravestone catalogue. Its values are a single placement index.
    /// </summary>
    [JsonPropertyName("gravestone")]
    public IReadOnlyDictionary<string, JsonElement> Gravestone { get; init; } = new Dictionary<string, JsonElement>(StringComparer.Ordinal);

    /// <summary>
    ///     The hair catalogue. Its values are the pair of grid offsets the hair sits at on its sheet.
    /// </summary>
    [JsonPropertyName("hair")]
    public IReadOnlyDictionary<string, JsonElement> Hair { get; init; } = new Dictionary<string, JsonElement>(StringComparer.Ordinal);

    /// <summary>
    ///     The hat catalogue. Its values are a single placement index.
    /// </summary>
    [JsonPropertyName("hat")]
    public IReadOnlyDictionary<string, JsonElement> Hat { get; init; } = new Dictionary<string, JsonElement>(StringComparer.Ordinal);

    /// <summary>
    ///     The head catalogue. Most values name the small, medium and large sheets the head is drawn from.
    /// </summary>
    /// <remarks>
    ///     The values are not one shape: some carry a trailing number after the three sheet names.
    /// </remarks>
    [JsonPropertyName("head")]
    public IReadOnlyDictionary<string, JsonElement> Head { get; init; } = new Dictionary<string, JsonElement>(StringComparer.Ordinal);

    /// <summary>
    ///     How far down one body sprite wears its head, keyed by the body's sprite name. Everything placed off the head moves
    ///     down with it.
    /// </summary>
    [JsonPropertyName("head_y")]
    public IReadOnlyDictionary<string, double> HeadY { get; init; } = new Dictionary<string, double>(StringComparer.Ordinal);

    /// <summary>
    ///     Retired names, each mapped to the one that replaced it.
    /// </summary>
    [JsonPropertyName("map")]
    public IReadOnlyDictionary<string, string> Map { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    ///     The sprites drawn with no upper body. <see cref="Prop" /> carries the same tag as a separate key.
    /// </summary>
    [JsonPropertyName("no_upper")]
    public IReadOnlyList<string> NoUpper { get; init; } = [];

    /// <summary>
    ///     How a sprite is drawn, keyed by sprite name: whether it covers what is worn beneath, hides hair, is bulky or
    ///     slender, animates fast. The tags are free-form strings.
    /// </summary>
    [JsonPropertyName("prop")]
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Prop { get; init; }
        = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
}
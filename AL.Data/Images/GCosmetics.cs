#region
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Images;

/// <summary>
///     The wardrobe rules: which names stand for several sprites, which stand for another name, and where the sheets a
///     slot draws from are placed.
/// </summary>
/// <remarks>
///     Every table here is looked up ordinally, unlike the rest of the game data. The server's are plain JS objects and
///     its own lookups are therefore case-sensitive (node/server.js:4844), so a case-insensitive table would answer that a
///     differently-cased name is wearable and the emit built on that answer comes back
///     <c>
///         cx_not_found
///     </c>
///     . The defaults below are ordinal for that reason, and so is what System.Text.Json binds over them.
///     <br />
///     A cosmetic's slot is resolved through <see cref="GSprite.Type" />, never through these catalogues: only four slots
///     carry one, and the catalogues exist for the per-slot placement their values hold rather than to enumerate what may
///     be worn.
/// </remarks>
public sealed record GCosmetics
{
    /// <summary>
    ///     The names that stand for several sprites at once, each mapped to its members. A bundle name is not itself wearable
    ///     - the server expands it and keys the members in verbatim, without sending them back through <see cref="Map" />
    ///     (js/old_common_functions.js:279).
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
    /// <remarks>
    ///     These six are where the client starts every placement from (js/html.js:5849-5851,
    ///     <c>
    ///         :5917-5920
    ///     </c>
    ///     ). They are the whole reason a preview drawn from the catalogues alone sits wrong: a hat stacks on the head's
    ///     placement plus the head's own height plus the hair's, and none of those three are in the catalogues below.
    /// </remarks>
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
    ///     The head catalogue.
    /// </summary>
    /// <remarks>
    ///     <b>
    ///         The values are not one shape.
    ///     </b>
    ///     Most name the small, medium and large sheets the head is drawn from, but a third of them carry a trailing number
    ///     after those three. That is why this is <see cref="JsonElement" />: typing it as a list of strings binds most of the
    ///     table and throws on the rest, and it throws inside
    ///     <c>
    ///         GameData.Bind
    ///     </c>
    ///     , so the symptom is a bot that will not start rather than a head that will not draw. Whoever needs the values reads
    ///     the shape here first.
    /// </remarks>
    [JsonPropertyName("head")]
    public IReadOnlyDictionary<string, JsonElement> Head { get; init; } = new Dictionary<string, JsonElement>(StringComparer.Ordinal);

    /// <summary>
    ///     Retired names, each mapped to the one that replaced it.
    /// </summary>
    [JsonPropertyName("map")]
    public IReadOnlyDictionary<string, string> Map { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    ///     The sprites drawn with no upper body, which is also a value <see cref="Prop" /> carries for the same sprite. The
    ///     two are separate keys in the data and stay separate here.
    /// </summary>
    [JsonPropertyName("no_upper")]
    public IReadOnlyList<string> NoUpper { get; init; } = [];

    /// <summary>
    ///     How a sprite is drawn, keyed by sprite name: whether it covers what is worn beneath, hides hair, is bulky or
    ///     slender, animates fast. The tags are free-form strings rather than a closed set.
    /// </summary>
    [JsonPropertyName("prop")]
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Prop { get; init; }
        = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
}
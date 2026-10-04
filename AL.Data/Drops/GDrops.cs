#region
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Drops;

/// <summary>Represents the game's drop tables.</summary>
public sealed record GDrops
{
    /// <summary>
    ///     The constants behind a kill's gold reward, which scale the monster's own gold figure before it is multiplied by its
    ///     level and your share of the kill.
    /// </summary>
    [JsonPropertyName("gold")]
    public GGoldDrop Gold { get; init; } = new();

    /// <summary>What the konami code rolls on.</summary>
    [JsonPropertyName("konami")]
    public IReadOnlyList<GDrop> Konami { get; init; } = [];

    /// <summary>
    ///     The per-map and global tables, keyed by map accessor. These divide by the monster's HP multiplier where a monster's
    ///     own table does not, so a rate here is not comparable with a rate in <see cref="Monsters" />.
    /// </summary>
    [JsonPropertyName("maps")]
    public IReadOnlyDictionary<string, IReadOnlyList<GDrop>> Maps { get; init; }
        = new Dictionary<string, IReadOnlyList<GDrop>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Each monster's own drop table, keyed by monster accessor. Every entry is rolled separately on every kill, so a
    ///     table naming one item twice gives it two independent chances and the rates add.
    /// </summary>
    [JsonPropertyName("monsters")]
    public IReadOnlyDictionary<string, IReadOnlyList<GDrop>> Monsters { get; init; }
        = new Dictionary<string, IReadOnlyList<GDrop>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     A second table a few monsters carry, rolled beside their own only when the killer is on the server they call home.
    ///     Keyed by monster accessor; entries roll and scale exactly as <see cref="Monsters" /> do.
    /// </summary>
    [JsonPropertyName("monsters_home_server")]
    public IReadOnlyDictionary<string, IReadOnlyList<GDrop>> MonstersHomeServer { get; init; }
        = new Dictionary<string, IReadOnlyList<GDrop>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Every other table, keyed by the drop id an exchange or an opened chest rolls under. Most ids are item names; some,
    ///     such as <c>xN</c>, <c>f1</c> and <c>skins</c>, name no item.
    /// </summary>
    /// <remarks>Enriched property</remarks>
    [JsonIgnore]
    public IReadOnlyDictionary<string, IReadOnlyList<GDrop>> Tables { get; internal set; }
        = new Dictionary<string, IReadOnlyList<GDrop>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     The wire's remaining keys, held until <c>GameData.EnrichDrops</c> turns them into <see cref="Tables" />.
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? Unbound { get; init; }

    /// <summary>
    ///     The rolls the server adds to the global table while a seasonal event runs, keyed by the event's <c>G.events</c>
    ///     key. The server adds them to its own copy at boot, so they never reach a client.
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<GDrop>> EventGlobalDrops { get; }
        = new Dictionary<string, IReadOnlyList<GDrop>>(StringComparer.OrdinalIgnoreCase)
        {
            ["halloween"] =
            [
                new GDrop
                {
                    Rate = 0.00005f,
                    Name = "candy0"
                },
                new GDrop
                {
                    Rate = 0.00125f,
                    Name = "candy1"
                }
            ],
            ["holidayseason"] =
            [
                new GDrop
                {
                    Rate = 0.0006f,
                    Name = "ornament"
                },
                new GDrop
                {
                    Rate = 0.0018f,
                    Name = "mistletoe"
                },
                new GDrop
                {
                    Rate = 0.0005f,
                    Name = "candycane"
                },
                new GDrop
                {
                    Rate = 0.0001f,
                    Name = "xN",
                    IsChest = true
                },
                new GDrop
                {
                    Rate = 0.00000001f,
                    Name = "orbofsc"
                }
            ],
            ["lunarnewyear"] =
            [
                new GDrop
                {
                    Rate = 0.00005f,
                    Name = "brownenvelope"
                },
                new GDrop
                {
                    Rate = 0.000000005f,
                    Name = "5bucks"
                }
            ],
            ["valentines"] =
            [
                new GDrop
                {
                    Rate = 0.001f,
                    Name = "candypop"
                }
            ],
            ["egghunt"] =
            [
                new GDrop
                {
                    Rate = 0.000005f,
                    Name = "goldenegg"
                },
                new GDrop
                {
                    Rate = 0.009f,
                    Name = "eastereggs",
                    IsChest = true
                }
            ]
        };

    /// <summary>
    ///     The entries of the shipped global table that the server rolls only while the anniversary runs.
    /// </summary>
    private static readonly IReadOnlySet<string> AnniversaryDrops = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "anniversarygift",
        "slice_strawberry",
        "slice_citrus",
        "slice_honey",
        "slice_mint",
        "slice_blueberry",
        "slice_nightberry"
    };

    /// <summary>
    ///     Builds the global table as the server rolls it while <paramref name="events" /> run.
    /// </summary>
    /// <param name="events">
    ///     The <c>G.events</c> keys of the seasonal events running.
    /// </param>
    /// <returns>
    ///     The shipped global entries whose event is running or that need none, then each running event's own rolls.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">events</exception>
    public IReadOnlyList<GDrop> GetGlobalTable(IReadOnlySet<string> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        const string ANNIVERSARY = "anniversary";

        var anniversary = events.Contains(ANNIVERSARY);

        return (Maps.GetValueOrDefault("global") ?? []).Where(drop => anniversary || !AnniversaryDrops.Contains(drop.Name))
                                                       .Concat(events.SelectMany(name => EventGlobalDrops.GetValueOrDefault(name) ?? []))
                                                       .ToList();
    }
}

/// <summary>
///     Represents the constants in a kill's gold reward, <c>
///         round(1 + gold * (BASE + rand() * RANDOM)) * level * mult
///     </c>. The monster's own <c>gold</c> reaches a client only in the start frame's <c>base_gold</c> table.
/// </summary>
public sealed record GGoldDrop
{
    /// <summary>
    ///     The share of the monster's gold value paid on every kill.
    /// </summary>
    [JsonPropertyName("base")]
    public float Base { get; init; }

    /// <summary>
    ///     The share of the monster's gold value paid on a uniform roll on top of <see cref="Base" />.
    /// </summary>
    [JsonPropertyName("random")]
    public float Random { get; init; }

    /// <summary>
    ///     The chance the whole reward is multiplied by ten.
    /// </summary>
    [JsonPropertyName("x10")]
    public float X10 { get; init; }

    /// <summary>
    ///     The chance the whole reward is multiplied by fifty, rolled independently of <see cref="X10" />.
    /// </summary>
    [JsonPropertyName("x50")]
    public float X50 { get; init; }
}
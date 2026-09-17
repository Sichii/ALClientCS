#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     The daily dungeon's run state as the server last sent it: the clock, the party's shared purse, where the stairs
///     and objectives are, and the vote in progress if the run is paused for one. Times are server epoch milliseconds.
/// </summary>
public sealed record CaveState
{
    /// <summary>
    ///     Cave amber in the party's shared purse.
    /// </summary>
    [JsonPropertyName("amber")]
    public int Amber { get; init; }

    /// <summary>
    ///     The vote in progress or just resolved, if any. The run is paused while one is open.
    /// </summary>
    [JsonPropertyName("choice")]
    public CaveChoice? Choice { get; init; }

    /// <summary>
    ///     The stairs and the exit on the current floor.
    /// </summary>
    [JsonPropertyName("doors")]
    public IReadOnlyList<CaveDoor> Doors { get; init; } = [];

    /// <summary>
    ///     When the run's clock runs out, unless paused.
    /// </summary>
    [JsonPropertyName("expires")]
    public long Expires { get; init; }

    /// <summary>
    ///     The floor the character is on, from zero.
    /// </summary>
    [JsonPropertyName("floor")]
    public int Floor { get; init; }

    /// <summary>
    ///     Cave gold in the party's shared purse. It is spent inside and never becomes carried gold.
    /// </summary>
    [JsonPropertyName("gold")]
    public long Gold { get; init; }

    /// <summary>
    ///     Timed hunts a traveler set: kill the marked monsters before the deadline.
    /// </summary>
    [JsonPropertyName("hunts")]
    public IReadOnlyList<CaveHunt> Hunts { get; init; } = [];

    /// <summary>
    ///     The rooms to settle, across every floor. The required ones on the current floor unlock its stairs.
    /// </summary>
    [JsonPropertyName("objectives")]
    public IReadOnlyList<CaveObjective> Objectives { get; init; } = [];

    /// <summary>
    ///     Whether the run is paused for a vote. Nothing moves or fights while it is.
    /// </summary>
    [JsonPropertyName("paused")]
    public bool Paused { get; init; }

    /// <summary>
    ///     When the pause began.
    /// </summary>
    [JsonPropertyName("paused_at")]
    public long PausedAt { get; init; }

    /// <summary>
    ///     Practice bouts a duelist set: bring the named sparring partner down to the marked hp before the deadline.
    /// </summary>
    [JsonPropertyName("practice")]
    public IReadOnlyList<CavePractice> Practice { get; init; } = [];

    /// <summary>
    ///     Time left on the clock while paused; read <see cref="Expires" /> against the server clock otherwise.
    /// </summary>
    [JsonPropertyName("remaining_ms")]
    public long RemainingMs { get; init; }

    /// <summary>
    ///     Everything the party has been paid so far, newest last.
    /// </summary>
    [JsonPropertyName("rewards")]
    public IReadOnlyList<CaveReward> Rewards { get; init; } = [];

    /// <summary>
    ///     The run's id. Every floor of the run is filed under a map key spelled from it.
    /// </summary>
    [JsonPropertyName("run")]
    public string Run { get; init; } = null!;

    /// <summary>
    ///     The server's clock when this state was sent, for reading the deadlines against.
    /// </summary>
    [JsonPropertyName("server_time")]
    public long ServerTime { get; init; }

    /// <summary>
    ///     Supplies the party picked up, which some choices need: "tool", "lamp", "decoy" and the like.
    /// </summary>
    [JsonPropertyName("supplies")]
    public IReadOnlyList<string> Supplies { get; init; } = [];
}

/// <summary>
///     A stair or the exit on the current floor.
/// </summary>
public sealed record CaveDoor
{
    /// <summary>
    ///     Whether the door leads a floor deeper.
    /// </summary>
    [JsonPropertyName("down")]
    public bool Down { get; init; }

    /// <summary>
    ///     Whether the door is sealed until the floor's required objectives are done.
    /// </summary>
    [JsonPropertyName("locked")]
    public bool Locked { get; init; }

    /// <summary>
    ///     The map the door stands on, when it is not the current floor.
    /// </summary>
    [JsonPropertyName("map")]
    public string? Map { get; init; }

    /// <summary>
    ///     The map the door leads to: a floor's key, or "main" for the exit.
    /// </summary>
    [JsonPropertyName("to")]
    public string To { get; init; } = null!;

    [JsonPropertyName("x")]
    public float X { get; init; }

    [JsonPropertyName("y")]
    public float Y { get; init; }
}

/// <summary>
///     A room to settle: a monster camp, a floor keeper or an encounter.
/// </summary>
public sealed record CaveObjective
{
    /// <summary>
    ///     Whether the room is settled.
    /// </summary>
    [JsonPropertyName("done")]
    public bool Done { get; init; }

    /// <summary>
    ///     The floor the room is on, from zero.
    /// </summary>
    [JsonPropertyName("floor")]
    public int Floor { get; init; }

    /// <summary>
    ///     What kind of room: "farm" for a camp, "boss" for a keeper, an encounter's kind otherwise.
    /// </summary>
    [JsonPropertyName("kind")]
    public string Kind { get; init; } = null!;

    /// <summary>
    ///     The map the room is on, when the floor number alone does not say.
    /// </summary>
    [JsonPropertyName("map")]
    public string? Map { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    /// <summary>
    ///     Whether the floor's stairs stay sealed until this room is done.
    /// </summary>
    [JsonPropertyName("required")]
    public bool Required { get; init; }

    /// <summary>
    ///     On a camp, how many of its packs have been cleared.
    /// </summary>
    [JsonPropertyName("waves")]
    public int Waves { get; init; }

    [JsonPropertyName("x")]
    public float X { get; init; }

    [JsonPropertyName("y")]
    public float Y { get; init; }
}

/// <summary>
///     Something the party was paid.
/// </summary>
public sealed record CaveReward
{
    [JsonPropertyName("amber")]
    public int Amber { get; init; }

    [JsonPropertyName("gold")]
    public long Gold { get; init; }

    /// <summary>
    ///     Rises with every reward in the run, so a state can be compared against the last one seen.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("item")]
    public CaveItem? Item { get; init; }

    /// <summary>
    ///     Who received it, for a reward that went to one character rather than the purse.
    /// </summary>
    [JsonPropertyName("recipient")]
    public string? Recipient { get; init; }

    /// <summary>
    ///     The inventory slot the item landed in, when it went to a bag.
    /// </summary>
    [JsonPropertyName("slot")]
    public int? Slot { get; init; }

    /// <summary>
    ///     Where it went: "purse" for the shared purse, "inventory", "gold", "mail", or pending mail.
    /// </summary>
    [JsonPropertyName("where")]
    public string Where { get; init; } = null!;
}

/// <summary>
///     An item named in a reward or carried by a person in a scene.
/// </summary>
public sealed record CaveItem
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    [JsonPropertyName("q")]
    public int Quantity { get; init; } = 1;
}

/// <summary>
///     A timed hunt a traveler set.
/// </summary>
public sealed record CaveHunt
{
    /// <summary>
    ///     How many kills the hunt asks for.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; init; }

    [JsonPropertyName("deadline")]
    public long Deadline { get; init; }

    /// <summary>
    ///     Kills so far.
    /// </summary>
    [JsonPropertyName("kills")]
    public int Kills { get; init; }
}

/// <summary>
///     A practice bout a duelist set.
/// </summary>
public sealed record CavePractice
{
    [JsonPropertyName("deadline")]
    public long Deadline { get; init; }

    /// <summary>
    ///     The sparring partner's hp to bring it down to.
    /// </summary>
    [JsonPropertyName("hp")]
    public int Hp { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;
}

/// <summary>
///     A vote: an encounter put to the party, one vote each, a minute to choose, majority settles it early.
/// </summary>
public sealed record CaveChoice
{
    /// <summary>
    ///     When the vote closes and the fallback applies.
    /// </summary>
    [JsonPropertyName("deadline")]
    public long Deadline { get; init; }

    /// <summary>
    ///     What happens if no option wins.
    /// </summary>
    [JsonPropertyName("fallback")]
    public string? Fallback { get; init; }

    [JsonPropertyName("id")]
    public string Id { get; init; } = null!;

    /// <summary>
    ///     The replies on offer.
    /// </summary>
    [JsonPropertyName("options")]
    public IReadOnlyList<CaveOption> Options { get; init; } = [];

    /// <summary>
    ///     Who is speaking and who else is in the room, with their strength and what they carry.
    /// </summary>
    [JsonPropertyName("people")]
    public IReadOnlyList<CavePerson> People { get; init; } = [];

    /// <summary>
    ///     Whether the vote is settled. The run resumes when it is.
    /// </summary>
    [JsonPropertyName("resolved")]
    public bool Resolved { get; init; }

    /// <summary>
    ///     The reply that won, once resolved.
    /// </summary>
    [JsonPropertyName("result_label")]
    public string? ResultLabel { get; init; }

    /// <summary>
    ///     A service the resolved choice opened: "recipes" for the collector's recipe list.
    /// </summary>
    [JsonPropertyName("service")]
    public string? Service { get; init; }

    /// <summary>
    ///     A merchant's one item, when the encounter is a shop.
    /// </summary>
    [JsonPropertyName("shop")]
    public CaveShop? Shop { get; init; }

    /// <summary>
    ///     What the resolution did, once resolved.
    /// </summary>
    [JsonPropertyName("summary")]
    public IReadOnlyList<string> Summary { get; init; } = [];

    [JsonPropertyName("text")]
    public string? Text { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    ///     Each character's vote so far, by name, as an option id.
    /// </summary>
    [JsonPropertyName("votes")]
    public IReadOnlyDictionary<string, string> Votes { get; init; } = new Dictionary<string, string>();
}

/// <summary>
///     One reply to a vote.
/// </summary>
public sealed record CaveOption
{
    /// <summary>
    ///     What it costs from the purse's amber.
    /// </summary>
    [JsonPropertyName("amber")]
    public int Amber { get; init; }

    /// <summary>
    ///     What it costs from the purse's gold.
    /// </summary>
    [JsonPropertyName("cost")]
    public long Cost { get; init; }

    [JsonPropertyName("id")]
    public string Id { get; init; } = null!;

    [JsonPropertyName("label")]
    public string? Label { get; init; }

    /// <summary>
    ///     Why the party cannot pick it, or null when it can.
    /// </summary>
    [JsonPropertyName("unavailable")]
    public string? Unavailable { get; init; }
}

/// <summary>
///     Someone in an encounter's room.
/// </summary>
public sealed record CavePerson
{
    [JsonPropertyName("attack")]
    public float Attack { get; init; }

    /// <summary>
    ///     What they carry, which is what a fight over them would pay.
    /// </summary>
    [JsonPropertyName("cargo")]
    public IReadOnlyList<CaveItem>? Cargo { get; init; }

    [JsonPropertyName("hp")]
    public int Hp { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;
}

/// <summary>
///     A merchant's one item. Bought from the purse, once, by whoever stands near enough.
/// </summary>
public sealed record CaveShop
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    /// <summary>
    ///     Whether this character stands close enough to buy.
    /// </summary>
    [JsonPropertyName("nearby")]
    public bool Nearby { get; init; }

    [JsonPropertyName("price")]
    public long Price { get; init; }

    /// <summary>
    ///     The room to name when buying.
    /// </summary>
    [JsonPropertyName("room")]
    public string Room { get; init; } = null!;

    [JsonPropertyName("sold")]
    public bool Sold { get; init; }
}

/// <summary>
///     A line a traveler said, on a chat frame or a talk reply.
/// </summary>
public sealed record CaveChat
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    [JsonPropertyName("text")]
    public string? Text { get; init; }
}

/// <summary>
///     A line an actor said in a room.
/// </summary>
public sealed record CaveCue
{
    /// <summary>
    ///     The entity id of the speaker.
    /// </summary>
    [JsonPropertyName("actor")]
    public string? Actor { get; init; }

    [JsonPropertyName("map")]
    public string? Map { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }
}

/// <summary>
///     The account's standing with the dungeon: one visit a day, shared by every character on the account.
/// </summary>
public sealed record CaveVisit
{
    /// <summary>
    ///     Whether the account can enter now.
    /// </summary>
    [JsonPropertyName("available")]
    public bool Available { get; init; }

    /// <summary>
    ///     The home server whose midnight resets the visit.
    /// </summary>
    [JsonPropertyName("home")]
    public string? Home { get; init; }

    /// <summary>
    ///     When the visit comes back.
    /// </summary>
    [JsonPropertyName("resets")]
    public long Resets { get; init; }

    [JsonPropertyName("server_time")]
    public long ServerTime { get; init; }

    /// <summary>
    ///     Whether visits are unlimited, as on a dev server.
    /// </summary>
    [JsonPropertyName("unlimited")]
    public bool Unlimited { get; init; }
}

/// <summary>
///     What a cave chest added to the party's shared purse.
/// </summary>
public sealed record CavePurse
{
    [JsonPropertyName("amber")]
    public int Amber { get; init; }

    [JsonPropertyName("gold")]
    public long Gold { get; init; }
}

/// <summary>
///     A monster's part in the dungeon: the travelers and actors are monsters with a side.
/// </summary>
public sealed record MonsterCave
{
    /// <summary>
    ///     Whether it is a passing traveler, who stops for a chat that pauses nothing.
    /// </summary>
    [JsonPropertyName("citizen")]
    public bool Citizen { get; init; }

    /// <summary>
    ///     The room it belongs to, to name when talking to it.
    /// </summary>
    [JsonPropertyName("room")]
    public string? Room { get; init; }

    /// <summary>
    ///     "neutral", "ally" or "victim" can be talked to; "enemy" and "predator" fight.
    /// </summary>
    [JsonPropertyName("side")]
    public string? Side { get; init; }
}

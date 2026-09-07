namespace AL.Client.Model;

/// <summary>
///     A monster this client could see has died.
/// </summary>
/// <param name="Id">
///     The entity id, unique to that one monster.
/// </param>
/// <param name="Type">
///     Its monster type, the key into game data.
/// </param>
/// <param name="Points">
///     For a cooperative boss, the server's final tally of points by character name, everyone included; null for any
///     other monster.
/// </param>
public sealed record MonsterDeathData(string Id, string Type, IReadOnlyDictionary<string, float>? Points = null);
namespace AL.Client.Model;

/// <summary>A monster this client could see has died.</summary>
/// <param name="Id">The entity id, unique to that one monster.</param>
/// <param name="Type">Its monster type, the key into game data.</param>
public sealed record MonsterDeathData(string Id, string Type);

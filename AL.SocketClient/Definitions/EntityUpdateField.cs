namespace AL.SocketClient.Definitions;

/// <summary>
///     Represents which wire keys an <c>entities</c> frame carried, so a delta only overwrites the fields it contained.
/// </summary>
/// <remarks>
///     The server's entity encoders are sparse: a soft property equal to the G default is omitted, and a state field is
///     sent only when defined. A bare <c>{id,x,y}</c> position delta must not zero a live monster's hp or speed.
/// </remarks>
[Flags]
public enum EntityUpdateField : uint
{
    None = 0,
    ABS = 1u << 0,
    Angle = 1u << 1,
    Armor = 1u << 2,
    Attack = 1u << 3,
    Conditions = 1u << 4,
    Focus = 1u << 5,
    Frequency = 1u << 6,
    GoingX = 1u << 7,
    GoingY = 1u << 8,
    HP = 1u << 9,

    /// <summary>
    ///     Carried, with <see cref="Map" />, only by a self <c>player</c> frame; a monster's or a stranger's map comes from
    ///     the frame-level stamp.
    /// </summary>
    In = 1u << 23,

    Level = 1u << 10,
    Map = 1u << 24,
    MaxHP = 1u << 11,
    MaxMP = 1u << 12,
    MoveNum = 1u << 13,
    Moving = 1u << 14,
    MP = 1u << 15,

    /// <summary>
    ///     Never carried by an entity frame; a monster's reach only ever comes from <c>G.monsters</c>, backfilled through the
    ///     same path as every other def-sourced value.
    /// </summary>
    Range = 1u << 22,

    Resistance = 1u << 16,
    Speed = 1u << 17,
    Target = 1u << 18,
    X = 1u << 19,
    XP = 1u << 20,
    Y = 1u << 21
}
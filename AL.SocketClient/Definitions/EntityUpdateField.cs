namespace AL.SocketClient.Definitions;

/// <summary>
///     Which wire keys an
///     <c>
///         entities
///     </c>
///     frame carried, captured during deserialization so a delta only overwrites the fields it actually contained. The
///     server's entity encoders are sparse (
///     <c>
///         node/server.js:878-928
///     </c>
///     ): a soft property equal to the G default is omitted, and a state field is sent only when defined. The browser
///     merges the same way - it iterates the received keys (
///     <c>
///         js/game.js:786
///     </c>
///     ), never a fixed whitelist - so a bare
///     <c>
///         {id,x,y}
///     </c>
///     position delta must not zero a live monster's hp/speed. Stored as a single value on the entity, so tracking
///     presence adds no heap allocation on this several-times-per-second-per-character hot path.
/// </summary>
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
    Level = 1u << 10,
    MaxHP = 1u << 11,
    MaxMP = 1u << 12,
    MoveNum = 1u << 13,
    Moving = 1u << 14,
    MP = 1u << 15,

    /// <summary>
    ///     Never actually carried by an entity frame - <c>monster_to_client</c> (<c>node/server.js:878</c>) lists it
    ///     in neither its soft set nor its hard one, so a monster's reach only ever comes from <c>G.monsters</c>. The
    ///     flag exists so that backfilling it runs through the same path as every other def-sourced value rather than
    ///     being a special case beside them, and so a frame that did carry one would still win.
    /// </summary>
    Range = 1u << 22,

    Resistance = 1u << 16,
    Speed = 1u << 17,
    Target = 1u << 18,
    X = 1u << 19,
    XP = 1u << 20,
    Y = 1u << 21
}
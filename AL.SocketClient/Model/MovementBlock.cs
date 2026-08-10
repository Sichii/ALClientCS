namespace AL.SocketClient.Model;

/// <summary>
///     Where an entity is and where it is walking, as one value. These travel together: a write that lands a
///     position without the destination it was derived from leaves the entity walking to a stale goal, so
///     <see cref="EntityBase" /> assigns them all at once under one lock and hands them out together here.
/// </summary>
/// <remarks>
///     <c>
///         Map
///     </c>
///     and
///     <c>
///         In
///     </c>
///     are in here because a position without the map it is on is meaningless:
///     <c>
///         DistanceWithInstanceCheck
///     </c>
///     answers <see cref="float.MaxValue" /> when the two disagree, and one frame of that evicts everything in
///     vision at once.
///     <br />
///     <c>
///         Speed
///     </c>
///     is deliberately not part of this. A stale speed makes one step slightly wrong, which is not a coherence break.
/// </remarks>
/// <seealso cref="EntityBase.Movement" />
public readonly record struct MovementBlock(
    float X,
    float Y,
    float GoingX,
    float GoingY,
    float Angle,
    ulong MoveNum,
    bool Moving,
    string? Map,
    string? In);

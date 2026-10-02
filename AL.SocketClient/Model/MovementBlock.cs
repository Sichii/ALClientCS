namespace AL.SocketClient.Model;

/// <summary>
///     Represents where an entity is and where it is walking, as one value. <see cref="EntityBase" /> assigns these
///     together under one lock, so a position never pairs with a destination it was not derived from.
/// </summary>
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
    string? In)
{
    /// <summary>
    ///     Determines whether this leg's direction has a positive component pointing away from <paramref name="other" />.
    /// </summary>
    /// <param name="other">The entity to compare against.</param>
    /// <returns>
    ///     <c>true</c> if this is moving and its leg points away from <paramref name="other" />; otherwise, <c>false</c> .
    /// </returns>
    /// <remarks>
    ///     Uses the leg from position to destination rather than <see cref="Angle" />, which can be stale. A leg whose
    ///     destination is where it already stands is not walking away.
    /// </remarks>
    public bool MovingAwayFrom(MovementBlock other) => Moving && (((GoingX - X) * (X - other.X) + (GoingY - Y) * (Y - other.Y)) > 0f);
}
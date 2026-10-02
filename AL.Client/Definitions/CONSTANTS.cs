namespace AL.Client.Definitions;

public static class CONSTANTS
{
    /// <summary>
    ///     The number of matching copies the compound bench takes per attempt.
    /// </summary>
    public const int ITEMS_PER_COMPOUND = 3;

    /// <summary>
    ///     The price the second-hands NPC charges for an ordinary item, as a multiple of its <c>g</c> value.
    /// </summary>
    /// <remarks>
    ///     The server values the item at <c>g * buy_to_sell</c> (0.6) and then applies a 2x second-hands multiplier.
    /// </remarks>
    public const float PONTY_MARKUP = 1.2f;

    /// <summary>
    ///     The price the second-hands NPC charges for a cash-shop item, as a multiple of its <c>g</c> value.
    /// </summary>
    /// <remarks>
    ///     Cash items skip the buy-to-sell discount and take a 3x multiplier instead of 2x, so the composite is a straight 3.
    /// </remarks>
    public const float PONTY_CASH_MARKUP = 3f;
}
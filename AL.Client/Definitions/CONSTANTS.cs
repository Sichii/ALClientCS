namespace AL.Client.Definitions
{
    public static class CONSTANTS
    {
        /// <summary>
        ///     What the second-hands NPC charges for an ordinary item, as a multiple of its "g" value.
        /// </summary>
        /// <remarks>
        ///     This is a composite and looks wrong at a glance. The server values the item at
        ///     <c>g * buy_to_sell</c> (0.6) and then applies a 2x second-hands multiplier, so the price
        ///     really is 1.2x the base value. Do not "correct" this to 2.
        /// </remarks>
        public const float PONTY_MARKUP = 1.2f;

        /// <summary>
        ///     The same, for cash-shop items.
        /// </summary>
        /// <remarks>
        ///     Cash items skip the buy-to-sell discount and take a 3x multiplier instead of 2x, so the
        ///     composite is a straight 3.
        /// </remarks>
        public const float PONTY_CASH_MARKUP = 3f;
    }
}

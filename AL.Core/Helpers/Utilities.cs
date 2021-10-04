using System;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Definitions;

namespace AL.Core.Helpers
{
    /// <summary>
    ///     A utility class for generally useful functions that don't belong attached to a specific object.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        ///     Calculates the damage multiplier that will be applied for a given defense value. (Does not account for the halving
        ///     of resistance for heals)
        /// </summary>
        /// <param name="defense">A resistance or armor value, after piercing has been accounted for.</param>
        /// <returns>
        ///     <see cref="float" /> <br />
        ///     The multiplier to be applied to the damage value to get the effective damage.
        /// </returns>
        public static float CalculateDamageMultiplier(float defense) => Math.Clamp(
            1
            - (Math.Clamp(defense, 0f, 100f) * 0.00100f
               + Math.Clamp(defense - 100, 0f, 100f) * 0.00100f
               + Math.Clamp(defense - 200, 0f, 100f) * 0.00095f
               + Math.Clamp(defense - 300, 0f, 100f) * 0.00090f
               + Math.Clamp(defense - 400, 0f, 100f) * 0.00082f
               + Math.Clamp(defense - 500, 0f, 100f) * 0.00070f
               + Math.Clamp(defense - 600, 0f, 100f) * 0.00060f
               + Math.Clamp(defense - 700, 0f, 100f) * 0.00050f
               + Math.Max(0f, defense - 800) * 0.00040f)
            + Math.Clamp(0 - defense, 0f, 50f) * 0.00100f
            + Math.Clamp(-50 - defense, 0f, 50f) * 0.00075f
            + Math.Clamp(-100 - defense, 0f, 50f) * 0.00050f
            + Math.Max(0f, -150 - defense) * 0.00025f, .05f, 1.32f);
    }
}
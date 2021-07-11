using System;

namespace AL.Core.Extensions
{
    /// <summary>
    ///     Provides a set of extensions for the <see cref="Math" /> class.
    /// </summary>
    public static class MathEx
    {
        /// <summary>
        ///     Calculates the length of the hypotenuse of a triangle.
        /// </summary>
        /// <param name="a">The lenth of side a of a triangle.</param>
        /// <param name="b">The length of side b of a triangle.</param>
        /// <returns>
        ///     <see cref="float" /> <br />
        ///     The length of the hyptenuse(c).
        /// </returns>
        public static float Hypot(float a, float b) => (float) Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
    }
}
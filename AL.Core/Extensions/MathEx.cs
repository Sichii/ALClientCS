using System;

namespace AL.Core.Extensions
{
    public static class MathEx
    {
        public static float Hypot(float d1, float d2) => (float) Math.Sqrt(Math.Pow(d1, 2) + Math.Pow(d2, 2));

        public static float Lerp(float start, float end, float maxMove, float minDiff)
        {
            var diff = end - start;

            if (maxMove > 0)
                diff = Math.Max(Math.Min(diff, maxMove), -maxMove);

            if (Math.Abs(diff) < minDiff)
                return end;

            return start + diff;
        }
    }
}
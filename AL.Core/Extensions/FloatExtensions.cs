using System;
using AL.Core.Definitions;

namespace AL.Core.Extensions
{
    public static class FloatExtensions
    {
        public static bool NearlyEqualOrGreaterThan(this float num1, float num2, float epsilon = CONSTANTS.EPSILON) =>
            num1 >= num2 || num1.NearlyEquals(num2, epsilon);

        public static bool NearlyEqualOrLessThan(this float num1, float num2, float epsilon = CONSTANTS.EPSILON) =>
            num1 <= num2 || num1.NearlyEquals(num2, epsilon);

        public static bool NearlyEquals(this float num1, float num2, float epsilon = CONSTANTS.EPSILON)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (num1 == num2)
                return true;

            return Math.Abs(num1 - num2) < epsilon;
        }

        public static bool SignificantlyGreaterThan(this float num1, float num2, float epsilon = CONSTANTS.EPSILON) =>
            num1 > num2 && !num1.NearlyEquals(num2, epsilon);

        public static bool SignificantlyLessThan(this float num1, float num2, float epsilon = CONSTANTS.EPSILON) =>
            num1 < num2 && !num1.NearlyEquals(num2, epsilon);
    }
}
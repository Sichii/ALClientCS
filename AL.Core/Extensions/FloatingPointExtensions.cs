#region
using System;
#endregion

namespace AL.Core.Extensions;

public static class FloatingPointExtensions
{
    public static bool IsGreater(this double a, double b, double epsilon = 0.0000001) => (a - b) > epsilon;

    public static bool IsGreater(this float a, float b, float epsilon = 0.0000001f) => (a - b) > epsilon;

    // ReSharper disable once CompareOfFloatsByEqualityOperator
    public static bool IsGreaterOrEqual(this double a, double b, double epsilon = 0.0000001) => (a == b) || IsGreater(a, b, epsilon);

    // ReSharper disable once CompareOfFloatsByEqualityOperator
    public static bool IsGreaterOrEqual(this float a, float b, float epsilon = 0.0000001f) => (a == b) || IsGreater(a, b, epsilon);

    public static bool IsLess(this double a, double b, double epsilon = 0.0000001) => (b - a) > epsilon;

    public static bool IsLess(this float a, float b, float epsilon = 0.0000001f) => (b - a) > epsilon;

    // ReSharper disable once CompareOfFloatsByEqualityOperator
    public static bool IsLessOrEqual(this double a, double b, double epsilon = 0.0000001) => (a == b) || IsLess(a, b, epsilon);

    // ReSharper disable once CompareOfFloatsByEqualityOperator
    public static bool IsLessOrEqual(this float a, float b, float epsilon = 0.0000001f) => (a == b) || IsLess(a, b, epsilon);

    public static bool IsNear(this double a, double b, double epsilon = 0.0000001) => Math.Abs(a - b) < epsilon;

    public static bool IsNear(this float a, float b, float epsilon = 0.0000001f) => Math.Abs(a - b) < epsilon;
}
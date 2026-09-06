#region
using System.Collections;
#endregion

namespace AL.Core.Geometry;

/// <summary>
///     Enumerates every grid cell a straight line crosses, one cell per step, without allocating.
///     <br />
///     https://playtechs.blogspot.com/2007/03/raytracing-on-grid.html
/// </summary>
public struct RayTrace : IEnumerable<Point>, IEnumerator<Point>
{
    private readonly float Dx;
    private readonly float Dy;
    private readonly int XOffset;
    private readonly int YOffset;
    private float Error;
    private int Remaining;
    private bool Started;
    private int X;
    private int Y;

    /// <summary>
    ///     Initializes a ray trace stepping from (x0, y0) toward (x1, y1).
    /// </summary>
    public RayTrace(
        float x0,
        float y0,
        float x1,
        float y1)
    {
        Dx = Math.Abs(x1 - x0);
        Dy = Math.Abs(y1 - y0);
        X = (int)Math.Floor(x0);
        Y = (int)Math.Floor(y0);
        var steps = 1;

        if (Dx == 0)
        {
            XOffset = 0;
            Error = float.PositiveInfinity;
        } else if (x1 > x0)
        {
            XOffset = 1;
            steps += (int)Math.Floor(x1) - X;
            Error = (float)(Math.Floor(x0) + 1 - x0) * Dy;
        } else
        {
            XOffset = -1;
            steps += X - (int)Math.Floor(x1);
            Error = (float)(x0 - Math.Floor(x0)) * Dy;
        }

        if (Dy == 0)
        {
            YOffset = 0;
            Error -= float.PositiveInfinity;
        } else if (y1 > y0)
        {
            YOffset = 1;
            steps += (int)Math.Floor(y1) - Y;
            Error -= (float)(Math.Floor(y0) + 1 - y0) * Dx;
        } else
        {
            YOffset = -1;
            steps += Y - (int)Math.Floor(y1);
            Error -= (float)(y0 - Math.Floor(y0)) * Dx;
        }

        Remaining = steps;
        Started = false;
    }

    /// <summary>
    ///     The grid cell at the ray trace's current position.
    /// </summary>
    public readonly Point Current => new(X, Y);

    readonly object IEnumerator.Current => Current;

    /// <summary>
    ///     Advances to the next grid cell the line crosses, returning whether a cell remains.
    /// </summary>
    public bool MoveNext()
    {
        if (Remaining <= 0)
            return false;

        //the first cell is the start's own; every later one is a step along whichever axis the error favours
        if (Started)
        {
            if (Error > 0)
            {
                Y += YOffset;
                Error -= Dx;
            } else
            {
                X += XOffset;
                Error += Dy;
            }
        }

        Started = true;
        Remaining--;

        return true;
    }

    /// <summary>
    ///     Not supported; a ray trace cannot be rewound, so this always throws.
    /// </summary>
    public void Reset() => throw new NotSupportedException();

    /// <summary>
    ///     Does nothing; present only because <see cref="IEnumerator{T}" /> requires <see cref="IDisposable" />.
    /// </summary>
    public readonly void Dispose() { }

    //returns a copy, so a foreach starts from wherever this instance stands. A fresh RayTraceTo call is the way to restart
    /// <summary>
    ///     Returns a copy of this ray trace, so enumerating it a second time restarts from the first cell.
    /// </summary>
    public readonly RayTrace GetEnumerator() => this;

    readonly IEnumerator<Point> IEnumerable<Point>.GetEnumerator() => this;

    readonly IEnumerator IEnumerable.GetEnumerator() => this;
}
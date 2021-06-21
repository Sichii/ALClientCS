using System;
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Interfaces;
using Chaos.Core.Extensions;

namespace AL.Core.Geometry
{
    public record Circle(float X, float Y, float Radius) : IPoint
    {
        public Circle(IPoint center, float radius)
            : this(center.X, center.Y, radius) { }

        public bool Contains(Circle other) => Radius >= Distance(other) + other.Radius;
        public float Distance(Circle other) => Math.Max(0, this.Distance((IPoint) other) - (Radius + other.Radius));

        public IEnumerable<Point> GenerateCircumferencePoints(float numberOfPoints, float startingAngle = 0f)
        {
            var anglePerPoint = 360 / numberOfPoints;

            for (var traversedAngle = 0f; traversedAngle.SignificantlyLessThan(360, CONSTANTS.EPSILON);
                traversedAngle += anglePerPoint)
                yield return this.AngularOffset(startingAngle + traversedAngle, Radius);
        }

        public override int GetHashCode() => HashCode.Combine(X, Y, Radius);

        public bool Intersects(Circle other) => this.Distance((IPoint) other) <= Radius + other.Radius;

        public IPoint LineIntersection(ILine line)
        {
            var start = line.Point1;
            var end = line.Point2;

            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            var sqr = dx * dx + dy * dy;
            var lne = 2 * (dx * (start.X - X) + dy * (start.Y - Y));
            var quad = (start.X - X) * (start.X - X) + (start.Y - Y) * (start.Y - Y) - Radius * Radius;

            var descriminant = lne * lne - 4 * sqr * quad;
            if (sqr <= 0.0000001 || descriminant < 0)
                return default;

            descriminant = (float) Math.Sqrt(descriminant);

            if (descriminant > 0)
            {
                // Two solutions.
                var t = (-lne - descriminant) / (2 * sqr);

                if (t is >= 0 and <= 1)
                    return new Point(start.X + t * dx, start.Y + t * dy);

                //t = (-lne + descriminant) / (2 * sqr);
                //yield return new Point(start.X + t * dx, start.Y + t * dy);
            }

            return default;
        }

        public IEnumerable<IPoint> Points(float numberOfSteps)
        {
            var stepSize = Radius / numberOfSteps * 2;

            for (var x = X - Radius; x <= X; x += stepSize)
                for (var y = Y - Radius; y <= Y; y += stepSize)
                {
                    var xdc = x - X;
                    var ydc = y - Y;

                    if (xdc * xdc + ydc * ydc <= Radius * Radius)
                    {
                        var xS = X - xdc;
                        var yS = Y - ydc;

                        yield return new Point(x, y);
                        yield return new Point(x, yS);
                        yield return new Point(xS, y);
                        yield return new Point(xS, yS);
                    }
                }
        }
    }
}
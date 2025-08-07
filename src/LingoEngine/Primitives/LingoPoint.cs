using System;
using System.Collections.Generic;
using System.Linq;

namespace LingoEngine.Primitives
{
    public struct LingoPoint : IEquatable<LingoPoint>
    {
        public float X { get; set; }
        public float Y { get; set; }

        public LingoPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public void Offset(float offsetX, float offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        public static LingoPoint operator +(LingoPoint a, LingoPoint b) =>
            new(a.X + b.X, a.Y + b.Y);

        public static LingoPoint operator -(LingoPoint a, LingoPoint b) =>
            new(a.X - b.X, a.Y - b.Y);

        public static LingoPoint operator *(LingoPoint p, float scalar) =>
            new(p.X * scalar, p.Y * scalar);

        public static LingoPoint operator /(LingoPoint p, float scalar) =>
            new(p.X / scalar, p.Y / scalar);

        public override string ToString() => $"({X}, {Y})";

        public override bool Equals(object? obj) => obj is LingoPoint point && Equals(point);

        public bool Equals(LingoPoint other) => X == other.X && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(LingoPoint left, LingoPoint right) => left.Equals(right);

        public static bool operator !=(LingoPoint left, LingoPoint right) => !(left == right);

        public static implicit operator LingoPoint((float x, float y) tuple)
            => new LingoPoint(tuple.x, tuple.y);

        public static LingoRect RectFromPoints(LingoPoint a, LingoPoint b)
        {
            float minX = Math.Min(a.X, b.X);
            float minY = Math.Min(a.Y, b.Y);
            float maxX = Math.Max(a.X, b.X);
            float maxY = Math.Max(a.Y, b.Y);
            return LingoRect.New(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        public static IEnumerable<LingoPoint> PointsInsidePolygon(IReadOnlyList<LingoPoint> poly)
        {
            float minX = poly.Min(p => p.X);
            float maxX = poly.Max(p => p.X);
            float minY = poly.Min(p => p.Y);
            float maxY = poly.Max(p => p.Y);

            for (int y = (int)minY; y <= (int)maxY; y++)
                for (int x = (int)minX; x <= (int)maxX; x++)
                {
                    var point = new LingoPoint(x, y);
                    if (point.IsInPolygon(poly))
                        yield return point;
                }
        }

        public bool IsInPolygon(IReadOnlyList<LingoPoint> poly)
        {
            bool inside = false;
            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                var pi = poly[i];
                var pj = poly[j];
                bool intersect = ((pi.Y > Y) != (pj.Y > Y)) &&
                                 (X < (pj.X - pi.X) * (Y - pi.Y) / (pj.Y - pi.Y) + pi.X);
                if (intersect)
                    inside = !inside;
            }
            return inside;
        }
    }
}

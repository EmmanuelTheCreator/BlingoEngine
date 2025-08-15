namespace AbstUI.Primitives
{
    using System;
    using System.Globalization;

    public struct ARect : IEquatable<ARect>
    {
        public float Left { get; set; }
        public float Top { get; set; }
        public float Right { get; set; }
        public float Bottom { get; set; }

        public ARect(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
        public static ARect New(float x, float y, float width, float height)
            => new ARect(x, y, x + width, y + height);

        public float Width => Right - Left;
        public float Height => Bottom - Top;
        public APoint TopLeft => new(Left, Top);
        public APoint BottomRight => new(Right, Bottom);
        public APoint Center => new((Left + Right) / 2, (Top + Bottom) / 2);

        public void Offset(float dx, float dy)
        {
            Left += dx;
            Right += dx;
            Top += dy;
            Bottom += dy;
        }

        public bool Contains(APoint point) =>
            point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;

        public bool Intersects(ARect other) =>
            !(Right < other.Left || Left > other.Right || Bottom < other.Top || Top > other.Bottom);

        public ARect Intersect(ARect other)
        {
            if (!Intersects(other)) return default;
            return new ARect(
                Math.Max(Left, other.Left),
                Math.Max(Top, other.Top),
                Math.Min(Right, other.Right),
                Math.Min(Bottom, other.Bottom)
            );
        }

        public ARect Union(ARect other)
        {
            return new ARect(
                Math.Min(Left, other.Left),
                Math.Min(Top, other.Top),
                Math.Max(Right, other.Right),
                Math.Max(Bottom, other.Bottom)
            );
        }

        public ARect Inset(float dx, float dy)
        {
            return new ARect(Left + dx, Top + dy, Right - dx, Bottom - dy);
        }

        public override string ToString() =>
            $"Rect({Left}, {Top}, {Right}, {Bottom})";

        public string ToCsv() =>
            $"{Left.ToString(CultureInfo.InvariantCulture)}," +
            $"{Top.ToString(CultureInfo.InvariantCulture)}," +
            $"{Right.ToString(CultureInfo.InvariantCulture)}," +
            $"{Bottom.ToString(CultureInfo.InvariantCulture)}";

        public static ARect Parse(string csv)
        {
            var parts = csv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(p => p.Trim())
               .ToArray();
            if (parts.Length != 4)
                throw new FormatException("AbstUIRect.Parse expects 4 comma-separated values.");

            return new ARect(
                float.Parse(parts[0], CultureInfo.InvariantCulture),
                float.Parse(parts[1], CultureInfo.InvariantCulture),
                float.Parse(parts[2], CultureInfo.InvariantCulture),
                float.Parse(parts[3], CultureInfo.InvariantCulture)
            );
        }
        public static ARect FromPoints(params APoint[] points)
        {
            if (points.Length == 0)
                return default;

            float left = points[0].X;
            float top = points[0].Y;
            float right = points[0].X;
            float bottom = points[0].Y;

            for (int i = 1; i < points.Length; i++)
            {
                var p = points[i];
                if (p.X < left) left = p.X;
                if (p.X > right) right = p.X;
                if (p.Y < top) top = p.Y;
                if (p.Y > bottom) bottom = p.Y;
            }

            return new ARect(left, top, right, bottom);
        }

        public override bool Equals(object? obj) => obj is ARect rect && Equals(rect);

        public bool Equals(ARect other) =>
            Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;

        public override int GetHashCode() => HashCode.Combine(Left, Top, Right, Bottom);

        public static bool operator ==(ARect left, ARect right) => left.Equals(right);
        public static bool operator !=(ARect left, ARect right) => !(left == right);
    }

}
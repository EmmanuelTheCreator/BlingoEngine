#if NET48
using System;

namespace System
{
    internal static class MathF
    {
        public const float PI = (float)Math.PI;
        public static float Pow(float x, float y) => (float)Math.Pow(x, y);
        public static float Tan(float x) => (float)Math.Tan(x);
        public static float Cos(float x) => (float)Math.Cos(x);
        public static float Sin(float x) => (float)Math.Sin(x);
        public static float Min(float x, float y) => Math.Min(x, y);
        public static float Max(float x, float y) => Math.Max(x, y);
        public static float Ceiling(float x) => (float)Math.Ceiling(x);
    }

    internal static class MathCompat
    {
        public static int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max);
        public static float Clamp(float value, float min, float max) => Math.Min(Math.Max(value, min), max);
    }

    internal struct HashCode
    {
        private int _value;
        public void Add<T>(T value) => _value = (_value * 31) + (value?.GetHashCode() ?? 0);
        public int ToHashCode() => _value;
        public static int Combine<T1, T2>(T1 v1, T2 v2)
        {
            int hash = 17;
            hash = hash * 31 + (v1?.GetHashCode() ?? 0);
            hash = hash * 31 + (v2?.GetHashCode() ?? 0);
            return hash;
        }
        public static int Combine<T1, T2, T3, T4>(T1 v1, T2 v2, T3 v3, T4 v4)
        {
            int hash = 17;
            hash = hash * 31 + (v1?.GetHashCode() ?? 0);
            hash = hash * 31 + (v2?.GetHashCode() ?? 0);
            hash = hash * 31 + (v3?.GetHashCode() ?? 0);
            hash = hash * 31 + (v4?.GetHashCode() ?? 0);
            return hash;
        }
    }
}
#endif

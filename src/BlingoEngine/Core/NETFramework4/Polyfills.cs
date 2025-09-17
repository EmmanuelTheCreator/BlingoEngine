using System;
using System.Runtime.CompilerServices;

namespace System
{
#if NET48
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
#endif

    internal static class MathCompat
    {
#if NET48
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max) => Math.Clamp(value, min, max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double value, double min, double max) => Math.Clamp(value, min, max);
#endif
    }
}

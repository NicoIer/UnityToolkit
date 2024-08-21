using System;
using System.Runtime.CompilerServices;

namespace UnityToolkit
{
    public static class ToolkitMath
    {
        public const float FloatMinNormal = 1.1754944E-38f;
        public const float FloatMinDenormal = float.Epsilon;
        public const bool IsFlushToZeroEnabled = FloatMinDenormal == 0.0;

        public static readonly float epsilon =
            IsFlushToZeroEnabled ? ToolkitMath.FloatMinNormal : ToolkitMath.FloatMinDenormal;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(double a, double b)
        {
            return Abs(b - a) < Max(1E-06f * Max(Abs(a), Abs(b)), epsilon * 8f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Max(double a, double b)
        {
            return Math.Max(a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Abs(double a)
        {
            return Math.Abs(a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>Clamps value between 0 and 1 and returns value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp01(double value) => Clamp(value, 0, 1);

        /// <summary>Calculates the linear parameter t that produces the interpolant value within the range [a, b].</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double InverseLerp(double a, double b, double value) =>
            !Approximately(a, b) ? Clamp01((value - a) / (b - a)) : 0;

        /// <summary>Linearly interpolates between a and b by t with no limit to t.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double LerpUnclamped(double a, double b, double t) =>
            a + (b - a) * t;

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Acos(float dot)
        {
            return MathF.Acos(dot);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin(float angle)
        {
            return MathF.Sin(angle);
        }
    }
}
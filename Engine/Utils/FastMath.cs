using System.Runtime.CompilerServices;

namespace MinecraftEngine.Engine.Utils;

public static class FastMath {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sin(float x) {
        x *= 0.159154943f;
        x -= 0.5f;
        var y = x - (int)(x + 0.5f);
        y *= 6.28318531f;
        return FastSin(y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Cos(float x) {
        x *= 0.159154943f;
        x -= 0.5f;
        var y = x - (int)(x + 0.5f);
        y *= 6.28318531f;
        return FastCos(y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FastSin(float x) {
        var x2 = x * x;
        var x3 = x2 * x;
        return x - x3 / 6f + x2 * x3 / 120f - x2 * x2 * x3 / 5040f + x2 * x2 * x2 * x3 / 362880f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FastCos(float x) {
        var x2 = x * x;
        return 1f - x2 / 2f + x2 * x2 / 24f - x2 * x2 * x2 / 720f + x2 * x2 * x2 * x2 / 40320f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sqrt(float x) => MathF.Sqrt(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Pow(float x, float y) => MathF.Pow(x, y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Floor(float x) => (int)Math.Floor(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Ceil(float x) => (int)Math.Ceiling(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Clamp(float value, float min, float max) => Math.Clamp(value, min, max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Lerp(float a, float b, float t) => a + t * (b - a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Smoothstep(float a, float b, float t) {
        var x = Clamp((t - a) / (b - a), 0f, 1f);
        return x * x * (3f - 2f * x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float PerlinFade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);
}
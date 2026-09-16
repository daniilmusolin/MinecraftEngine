using System.Runtime.CompilerServices;

namespace MinecraftEngine.Engine.Terrain;

public class SimplexNoise {
    private readonly int[] _perm = new int[512];
    private readonly int[] _permMod12 = new int[512];

    public SimplexNoise(int seed) {
        var random = new Random(seed);
        var p = new int[256];
        for (var i = 0; i < 256; i++) p[i] = i;
        
        for (var i = 0; i < 256; i++) {
            var j = random.Next(256);
            (p[i], p[j]) = (p[j], p[i]);
        }
        
        for (var i = 0; i < 512; i++) {
            _perm[i] = p[i & 255];
            _permMod12[i] = (byte)(_perm[i] % 12);
        }
    }

    public float Fractal(float x, float y, int octaves = 4, float lacunarity = 2f, float gain = 0.5f) {
        var value = 0f;
        var amplitude = 1f;
        var frequency = 1f;
        var maxValue = 0f;

        for (var i = 0; i < octaves; i++) {
            value += amplitude * Sample(x * frequency, y * frequency);
            maxValue += amplitude;
            amplitude *= gain;
            frequency *= lacunarity;
        }

        return value / maxValue;
    }

    public float Fractal(float x, float y, float z, int octaves = 4, float lacunarity = 2f, float gain = 0.5f) {
        var value = 0f;
        var amplitude = 1f;
        var frequency = 1f;
        var maxValue = 0f;

        for (var i = 0; i < octaves; i++) {
            value += amplitude * Sample(x * frequency, y * frequency, z * frequency);
            maxValue += amplitude;
            amplitude *= gain;
            frequency *= lacunarity;
        }

        return value / maxValue;
    }

    public float Sample(float x, float y) {
        const float F2 = 0.3660254037844386f; // (sqrt(3)-1)/2
        const float G2 = 0.21132486540518713f; // (3-sqrt(3))/6

        var s = (x + y) * F2;
        var i = (int)Math.Floor(x + s);
        var j = (int)Math.Floor(y + s);
        var t = (i + j) * G2;
        var x0 = x - i + t;
        var y0 = y - j + t;

        var i1 = x0 > y0 ? 1 : 0;
        var j1 = x0 > y0 ? 0 : 1;

        var x1 = x0 - i1 + G2;
        var y1 = y0 - j1 + G2;
        var x2 = x0 - 1f + 2f * G2;
        var y2 = y0 - 1f + 2f * G2;

        var ii = i & 255;
        var jj = j & 255;

        var n0 = 0f;
        var t0 = 0.5f - x0 * x0 - y0 * y0;
        if (t0 >= 0f) {
            t0 *= t0;
            n0 = t0 * t0 * Dot2(_perm[ii + _perm[jj]], x0, y0);
        }

        var n1 = 0f;
        var t1 = 0.5f - x1 * x1 - y1 * y1;
        if (t1 >= 0f) {
            t1 *= t1;
            n1 = t1 * t1 * Dot2(_perm[ii + i1 + _perm[jj + j1]], x1, y1);
        }

        var n2 = 0f;
        var t2 = 0.5f - x2 * x2 - y2 * y2;
        if (t2 >= 0f) {
            t2 *= t2;
            n2 = t2 * t2 * Dot2(_perm[ii + 1 + _perm[jj + 1]], x2, y2);
        }

        return 70f * (n0 + n1 + n2);
    }

    public float Sample(float x, float y, float z) {
        const float F3 = 0.3333333333333333f;
        const float G3 = 0.16666666666666666f;

        var s = (x + y + z) * F3;
        var i = (int)Math.Floor(x + s);
        var j = (int)Math.Floor(y + s);
        var k = (int)Math.Floor(z + s);
        var t = (i + j + k) * G3;
        var x0 = x - i + t;
        var y0 = y - j + t;
        var z0 = z - k + t;

        int i1, j1, k1, i2, j2, k2;
        if (x0 >= y0) {
            if (y0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
            else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; }
            else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; }
        } else {
            if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; }
            else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; }
            else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
        }

        var x1 = x0 - i1 + G3;
        var y1 = y0 - j1 + G3;
        var z1 = z0 - k1 + G3;
        var x2 = x0 - i2 + 2f * G3;
        var y2 = y0 - j2 + 2f * G3;
        var z2 = z0 - k2 + 2f * G3;
        var x3 = x0 - 1f + 3f * G3;
        var y3 = y0 - 1f + 3f * G3;
        var z3 = z0 - 1f + 3f * G3;

        var ii = i & 255;
        var jj = j & 255;
        var kk = k & 255;

        var n0 = 0f;
        var t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0;
        if (t0 >= 0f) {
            t0 *= t0;
            n0 = t0 * t0 * Dot3(_perm[ii + _perm[jj + _perm[kk]]], x0, y0, z0);
        }

        var n1 = 0f;
        var t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1;
        if (t1 >= 0f) {
            t1 *= t1;
            n1 = t1 * t1 * Dot3(_perm[ii + i1 + _perm[jj + j1 + _perm[kk + k1]]], x1, y1, z1);
        }

        var n2 = 0f;
        var t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
        if (t2 >= 0f) {
            t2 *= t2;
            n2 = t2 * t2 * Dot3(_perm[ii + i2 + _perm[jj + j2 + _perm[kk + k2]]], x2, y2, z2);
        }

        var n3 = 0f;
        var t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
        if (t3 >= 0f) {
            t3 *= t3;
            n3 = t3 * t3 * Dot3(_perm[ii + 1 + _perm[jj + 1 + _perm[kk + 1]]], x3, y3, z3);
        }

        return 32f * (n0 + n1 + n2 + n3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Dot2(int h, float x, float y) {
        var u = (h & 1) == 0 ? x : -x;
        var v = (h & 2) == 0 ? y : -y;
        return u + v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Dot3(int h, float x, float y, float z) {
        var u = (h & 1) == 0 ? x : -x;
        var v = (h & 2) == 0 ? y : -y;
        var w = (h & 4) == 0 ? z : -z;
        return u + v + w;
    }
}
namespace MinecraftEngine.Engine.Terrain;

public class PerlinNoise {
    private readonly int[] _permutation = new int[512];

    public PerlinNoise(int seed) {
        var random = new Random(seed);
        var p = new int[256];
        for (var i = 0; i < 256; i++) {
            p[i] = i;
        }
        for (var i = 0; i < 256; i++) {
            var j = random.Next(256);
            (p[i], p[j]) = (p[j], p[i]);
        }
        for (var i = 0; i < 512; i++) {
            _permutation[i] = p[i & 255];
        }
    }

    public float Noise(float x, float y) => Noise(x, y, 0);

    public float Noise(float x, float y, float z) {
        var xi = (int)Math.Floor(x) & 255;
        var yi = (int)Math.Floor(y) & 255;
        var zi = (int)Math.Floor(z) & 255;

        var xf = x - (int)Math.Floor(x);
        var yf = y - (int)Math.Floor(y);
        var zf = z - (int)Math.Floor(z);

        var u = Fade(xf);
        var v = Fade(yf);
        var w = Fade(zf);

        var p = _permutation;
        var aaa = p[p[p[xi] + yi] + zi];
        var aba = p[p[p[xi] + yi + 1] + zi];
        var aab = p[p[p[xi] + yi] + zi + 1];
        var abb = p[p[p[xi] + yi + 1] + zi + 1];
        var baa = p[p[p[xi + 1] + yi] + zi];
        var bba = p[p[p[xi + 1] + yi + 1] + zi];
        var bab = p[p[p[xi + 1] + yi] + zi + 1];
        var bbb = p[p[p[xi + 1] + yi + 1] + zi + 1];

        var x1 = Lerp(Grad(aaa, xf, yf, zf), Grad(baa, xf - 1, yf, zf), u);
        var x2 = Lerp(Grad(aba, xf, yf - 1, zf), Grad(bba, xf - 1, yf - 1, zf), u);
        var x3 = Lerp(Grad(aab, xf, yf, zf - 1), Grad(bab, xf - 1, yf, zf - 1), u);
        var x4 = Lerp(Grad(abb, xf, yf - 1, zf - 1), Grad(bbb, xf - 1, yf - 1, zf - 1), u);

        var y1 = Lerp(x1, x2, v);
        var y2 = Lerp(x3, x4, v);

        return Lerp(y1, y2, w);
    }

    private static float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);
    private static float Lerp(float a, float b, float t) => a + t * (b - a);

    private static float Grad(int hash, float x, float y, float z) {
        var h = hash & 15;
        var u = h < 8 ? x : y;
        var v = h < 4 ? y : h == 12 || h == 14 ? x : z;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}
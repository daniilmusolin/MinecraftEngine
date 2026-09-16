namespace MinecraftEngine.Engine.Terrain;

public abstract class Noise {
    protected readonly int Seed;
    protected readonly Random Random;

    public Noise(int seed) {
        Seed = seed;
        Random = new Random(seed);
    }

    public abstract float Sample(float x, float y);
    public abstract float Sample(float x, float y, float z);

    // Фрактальный шум (для детализации)
    public virtual float Fractal(float x, float y, int octaves = 4, float lacunarity = 2f, float gain = 0.5f) {
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

    public virtual float Fractal(float x, float y, float z, int octaves = 4, float lacunarity = 2f, float gain = 0.5f) {
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
}